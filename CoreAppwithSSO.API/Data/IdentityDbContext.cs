using CoreAppwithSSO.API.Models.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Data;

public class IdentityDbContext : DbContext, IDataProtectionKeyContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(t => t.Slug).IsUnique();
            entity.HasIndex(t => t.DbName).IsUnique();
            entity.HasIndex(t => t.ClientCode).IsUnique();
            entity.Property(t => t.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<UserTenant>(entity =>
        {
            entity.HasKey(ut => new { ut.UserId, ut.TenantId });
            entity.Property(ut => ut.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(ut => ut.User)
                .WithMany(u => u.UserTenants)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ut => ut.Tenant)
                .WithMany(t => t.UserTenants)
                .HasForeignKey(ut => ut.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasIndex(k => k.KeyPrefix).IsUnique();

            // At most one active (non-revoked) key per tenant. Revoked rows
            // are excluded from the index so historical keys can coexist.
            entity.HasIndex(k => k.TenantId)
                .IsUnique()
                .HasFilter("[RevokedAt] IS NULL");

            entity.Property(k => k.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(k => k.Tenant)
                .WithMany()
                .HasForeignKey(k => k.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.Property(s => s.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
