using CoreAppwithSSO.ElectionTracker.Services.Interface;

namespace CoreAppwithSSO.ElectionTracker.Services.Implementation
{
    /// <summary>
    /// Builds a per-tenant connection string by substituting the tenant's physical database
    /// name into a single template (ConnectionStrings:TenantConnectionTemplate). The connection
    /// string itself is never stored per-tenant — only the DbName is, in the central catalog.
    /// Mirrors CoreAppwithSSO.API's TenantConnectionResolver so both apps resolve tenant DBs
    /// the same way.
    /// </summary>
    public class TenantConnectionResolver : ITenantConnectionResolver
    {
        private const string Placeholder = "{TenantDbName}";
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

            return _template.Replace(Placeholder, tenantDbName);
        }
    }
}
