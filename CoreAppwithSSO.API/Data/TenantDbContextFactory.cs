using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CoreAppwithSSO.API.Data;

/// <summary>
/// Design-time factory so EF tools can construct a TenantDbContext for migrations.
/// At runtime, TenantDbContext is built per-request from the resolved tenant connection string.
/// </summary>
public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var template = config.GetConnectionString("TenantConnectionTemplate")
            ?? throw new InvalidOperationException("Missing 'TenantConnectionTemplate' connection string.");

        // Use a placeholder DB name so migrations can be generated without a real tenant.
        var connStr = template.Replace("{TenantDbName}", "CoreAppwithSSO_Tenant_Template");

        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(connStr)
            .Options;

        return new TenantDbContext(options);
    }
}
