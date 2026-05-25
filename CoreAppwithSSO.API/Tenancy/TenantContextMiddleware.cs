using System.Security.Claims;
using CoreAppwithSSO.API.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Tenancy;

/// <summary>
/// Reads the 'tenant_id' claim from the authenticated principal and populates ITenantContext.
/// Must run AFTER UseAuthentication.
/// </summary>
public class TenantContextMiddleware
{
    public const string TenantIdClaim = "tenant_id";

    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, IdentityDbContext identity)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var raw = context.User.FindFirstValue(TenantIdClaim);
            if (int.TryParse(raw, out var tenantId))
            {
                var dbName = await identity.Tenants
                    .Where(t => t.Id == tenantId && t.IsActive)
                    .Select(t => t.DbName)
                    .FirstOrDefaultAsync();

                if (dbName != null)
                {
                    tenantContext.Set(tenantId, dbName);
                }
            }
        }

        await _next(context);
    }
}
