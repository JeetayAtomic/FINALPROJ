using CoreAppwithSSO.API.Models.Tenant;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Data;

/// <summary>
/// Per-tenant database. Every tenant has the same schema; each tenant owns a physical DB.
/// </summary>
public class TenantDbContext : DbContext
{
    /// <summary>Well-known seeded role Ids (deterministic per the <see cref="OnModelCreating"/> HasData calls).</summary>
    public const int AdminRoleId = 1;
    public const int UserRoleId = 2;
    public const string AdminRoleName = "Admin";
    public const string UserRoleName = "User";

    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RoleApplication> RoleApplications => Set<RoleApplication>();
    public DbSet<JsonTemplate> JsonTemplates => Set<JsonTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(r => r.Name).IsUnique();
            entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<RoleApplication>(entity =>
        {
            entity.HasKey(ra => new { ra.RoleId, ra.ApplicationId });

            entity.HasOne(ra => ra.Role)
                .WithMany(r => r.RoleApplications)
                .HasForeignKey(ra => ra.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JsonTemplate>(entity =>
        {
            entity.HasIndex(t => t.Name).IsUnique();
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Seed default system roles.
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = AdminRoleId, Name = AdminRoleName, Description = "Full tenant administrator", IsSystem = true, CreatedAt = seedDate },
            new Role { Id = UserRoleId, Name = UserRoleName, Description = "Standard user (no apps by default)", IsSystem = true, CreatedAt = seedDate }
        );
    }
}
