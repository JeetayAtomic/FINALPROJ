namespace CoreAppwithSSO.ElectionTracker.Models
{
    public class Tenant
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Database engine this tenant runs on. Bound from the "DbProvider" config key
        /// (e.g. "SqlServer" or "PostgreSql"); defaults to SqlServer when the key is
        /// absent so existing tenants keep working unchanged.
        /// </summary>
        public DbProvider DbProvider { get; set; } = DbProvider.SqlServer;
    }
}
