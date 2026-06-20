using CoreAppwithSSO.API.Models.Sso;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Data;

public class SsoDbContext : DbContext
{
    public SsoDbContext(DbContextOptions<SsoDbContext> options) : base(options) { }

    public DbSet<Application> Applications => Set<Application>();
    public DbSet<TenantApplication> TenantApplications => Set<TenantApplication>();
    public DbSet<SsoToken> SsoTokens => Set<SsoToken>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasIndex(a => a.Name).IsUnique();
            entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<TenantApplication>(entity =>
        {
            entity.HasKey(ta => new { ta.TenantId, ta.ApplicationId });
            entity.Property(ta => ta.AssignedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(ta => ta.Application)
                .WithMany(a => a.TenantApplications)
                .HasForeignKey(ta => ta.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SsoToken>(entity =>
        {
            entity.HasIndex(s => s.Token).IsUnique();
            entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(s => s.Application)
                .WithMany()
                .HasForeignKey(s => s.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(s => new { s.UserId, s.TenantId });

            entity.HasOne(s => s.SsoToken)
                .WithMany()
                .HasForeignKey(s => s.SsoTokenId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // No seed data: the Application catalog is populated at runtime via the
        // super-admin endpoints (POST /api/admin/applications). Register each app there.
    }
}
