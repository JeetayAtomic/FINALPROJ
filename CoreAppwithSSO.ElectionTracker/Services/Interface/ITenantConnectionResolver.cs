namespace CoreAppwithSSO.ElectionTracker.Services.Interface
{
    public interface ITenantConnectionResolver
    {
        /// <summary>Builds the SQL connection string for the given tenant database name.</summary>
        string Resolve(string tenantDbName);
    }
}
