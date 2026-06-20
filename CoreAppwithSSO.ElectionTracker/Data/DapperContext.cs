using CoreAppwithSSO.ElectionTracker.Data.Sql;
using CoreAppwithSSO.ElectionTracker.Models;
using CoreAppwithSSO.ElectionTracker.Services;
using Microsoft.Data.SqlClient;
using Npgsql;
using System.Data;

namespace CoreAppwithSSO.ElectionTracker.Data
{
    public class DapperContext
    {
        private readonly ITenantService _tenantService;
        public DapperContext(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Opens a new connection for the current tenant using the ADO.NET provider that
        /// matches the tenant's configured <see cref="DbProvider"/>.
        /// </summary>
        public IDbConnection CreateConnection()
        {
            var tenant = CurrentTenant();
            return tenant.DbProvider switch
            {
                DbProvider.PostgreSql => new NpgsqlConnection(tenant.ConnectionString),
                _ => new SqlConnection(tenant.ConnectionString)
            };
        }

        /// <summary>
        /// The SQL dialect for the current tenant. Repositories use this so the generated
        /// SQL matches the tenant's engine.
        /// </summary>
        public ISqlDialect Dialect => SqlDialectFactory.For(CurrentTenant().DbProvider);

        private Tenant CurrentTenant() =>
            _tenantService.GetCurrentTenant()
                ?? throw new InvalidOperationException(
                    "No tenant resolved for this request. Ensure X-Tenant-Id header is set.");
    }
}

