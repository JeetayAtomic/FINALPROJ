using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace CoreAppwithSSO.ElectionTracker.Services.Implementation
{
    /// <summary>
    /// Reads the tenant -> physical database name mapping from the central Tenants table
    /// (ConnectionStrings:IdentityConnection), the same catalog CoreAppwithSSO.API writes to
    /// when provisioning tenants. The mapping is effectively static, so results are cached.
    /// </summary>
    public class TenantCatalog : ITenantCatalog
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        private readonly string _identityConnectionString;
        private readonly IMemoryCache _cache;

        public TenantCatalog(IConfiguration config, IMemoryCache cache)
        {
            _identityConnectionString = config.GetConnectionString("IdentityConnection")
                ?? throw new InvalidOperationException("Missing 'IdentityConnection' connection string.");
            _cache = cache;
        }

        public async Task<string?> GetDbNameAsync(int tenantId)
        {
            var cacheKey = $"tenant-db:{tenantId}";
            if (_cache.TryGetValue<string>(cacheKey, out var cached))
                return cached;

            await using var connection = new SqlConnection(_identityConnectionString);
            var dbName = await connection.QuerySingleOrDefaultAsync<string?>(
                "SELECT DbName FROM Tenants WHERE Id = @tenantId AND IsActive = 1",
                new { tenantId });

            // Cache positive hits only; a miss may be a not-yet-provisioned tenant we want to
            // re-check on the next request rather than mask for the full TTL.
            if (!string.IsNullOrEmpty(dbName))
                _cache.Set(cacheKey, dbName, CacheTtl);

            return dbName;
        }
    }
}
