namespace CoreAppwithSSO.ElectionTracker.Services.Interface
{
    public interface ITenantCatalog
    {
        /// <summary>
        /// Returns the physical database name for an active tenant from the central Tenants
        /// catalog, or <c>null</c> when the tenant does not exist or is inactive.
        /// </summary>
        Task<string?> GetDbNameAsync(int tenantId);
    }
}
