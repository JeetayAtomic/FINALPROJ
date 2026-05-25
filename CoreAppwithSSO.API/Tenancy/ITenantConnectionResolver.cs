namespace CoreAppwithSSO.API.Tenancy;

public interface ITenantConnectionResolver
{
    /// <summary>Builds the SQL connection string for the given tenant database name.</summary>
    string Resolve(string tenantDbName);
}

public class TenantConnectionResolver : ITenantConnectionResolver
{
    private readonly string _template;

    public TenantConnectionResolver(IConfiguration config)
    {
        _template = config.GetConnectionString("TenantConnectionTemplate")
            ?? throw new InvalidOperationException("Missing 'TenantConnectionTemplate' connection string.");
    }

    public string Resolve(string tenantDbName)
    {
        if (string.IsNullOrWhiteSpace(tenantDbName))
            throw new ArgumentException("Tenant DB name is required.", nameof(tenantDbName));

        return _template.Replace("{TenantDbName}", tenantDbName);
    }
}
