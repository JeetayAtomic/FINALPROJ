namespace CoreAppwithSSO.API.Tenancy;

/// <summary>
/// Scoped per-request accessor for the current tenant.
/// Populated by <see cref="TenantContextMiddleware"/> from the JWT 'tenant_id' claim.
/// </summary>
public interface ITenantContext
{
    int? TenantId { get; }
    string? TenantDbName { get; }

    void Set(int tenantId, string tenantDbName);
}

public class TenantContext : ITenantContext
{
    public int? TenantId { get; private set; }
    public string? TenantDbName { get; private set; }

    public void Set(int tenantId, string tenantDbName)
    {
        TenantId = tenantId;
        TenantDbName = tenantDbName;
    }
}
