using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.Models.Sso;
using CoreAppwithSSO.API.Models.Tenant;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Tenancy;

public interface ITenantProvisioner
{
    /// <summary>
    /// Creates the physical tenant DB, applies migrations, subscribes the tenant to all
    /// active global applications, and grants the seeded "Admin" role access to those apps.
    /// </summary>
    Task ProvisionAsync(int tenantId, string tenantDbName, CancellationToken ct = default);
}

public class TenantProvisioner : ITenantProvisioner
{
    private readonly ITenantConnectionResolver _resolver;
    private readonly SsoDbContext _sso;

    public TenantProvisioner(ITenantConnectionResolver resolver, SsoDbContext sso)
    {
        _resolver = resolver;
        _sso = sso;
    }

    public async Task ProvisionAsync(int tenantId, string tenantDbName, CancellationToken ct = default)
    {
        // 1. Create + migrate tenant DB (Roles/RoleApplications + seeded roles).
        var connStr = _resolver.Resolve(tenantDbName);
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(connStr)
            .Options;

        await using var db = new TenantDbContext(options);
        await db.Database.MigrateAsync(ct);

        // 2. Subscribe to every active global application.
        var appIds = await _sso.Applications
            .Where(a => a.IsActive)
            .Select(a => a.Id)
            .ToListAsync(ct);

        foreach (var appId in appIds)
        {
            var exists = await _sso.TenantApplications
                .AnyAsync(ta => ta.TenantId == tenantId && ta.ApplicationId == appId, ct);
            if (!exists)
            {
                _sso.TenantApplications.Add(new TenantApplication
                {
                    TenantId = tenantId,
                    ApplicationId = appId,
                    IsActive = true
                });
            }
        }
        await _sso.SaveChangesAsync(ct);

        // 3. Grant the seeded "Admin" role access to every subscribed app.
        foreach (var appId in appIds)
        {
            var exists = await db.RoleApplications
                .AnyAsync(ra => ra.RoleId == TenantDbContext.AdminRoleId && ra.ApplicationId == appId, ct);
            if (!exists)
            {
                db.RoleApplications.Add(new RoleApplication
                {
                    RoleId = TenantDbContext.AdminRoleId,
                    ApplicationId = appId
                });
            }
        }
        await db.SaveChangesAsync(ct);
    }
}
