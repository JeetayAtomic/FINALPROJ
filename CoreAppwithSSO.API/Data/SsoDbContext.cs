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

        // Seed global app catalog
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<Application>().HasData(
            new Application { Id = 1, Name = "HR Portal", Description = "Human Resources Management", BaseUrl = "https://hr.yourcompany.com", IconName = "people", IconColor = "#4285F4", DisplayOrder = 1, CreatedAt = seedDate },
            new Application { Id = 2, Name = "Finance", Description = "Financial Management System", BaseUrl = "https://finance.yourcompany.com", IconName = "account_balance", IconColor = "#EA4335", DisplayOrder = 2, CreatedAt = seedDate },
            new Application { Id = 3, Name = "Project Mgmt", Description = "Project Management Tool", BaseUrl = "https://projects.yourcompany.com", IconName = "assignment", IconColor = "#FBBC05", DisplayOrder = 3, CreatedAt = seedDate },
            new Application { Id = 4, Name = "CRM", Description = "Customer Relationship Management", BaseUrl = "https://crm.yourcompany.com", IconName = "contacts", IconColor = "#34A853", DisplayOrder = 4, CreatedAt = seedDate },
            new Application { Id = 5, Name = "Inventory", Description = "Inventory Management System", BaseUrl = "https://inventory.yourcompany.com", IconName = "inventory_2", IconColor = "#FF6D01", DisplayOrder = 5, CreatedAt = seedDate },
            new Application { Id = 6, Name = "Reports", Description = "Business Intelligence & Reports", BaseUrl = "https://reports.yourcompany.com", IconName = "analytics", IconColor = "#46BDC6", DisplayOrder = 6, CreatedAt = seedDate },
            new Application { Id = 7, Name = "Email", Description = "Corporate Email System", BaseUrl = "https://mail.yourcompany.com", IconName = "email", IconColor = "#7B1FA2", DisplayOrder = 7, CreatedAt = seedDate },
            new Application { Id = 8, Name = "Calendar", Description = "Team Calendar & Scheduling", BaseUrl = "https://calendar.yourcompany.com", IconName = "calendar_month", IconColor = "#0097A7", DisplayOrder = 8, CreatedAt = seedDate },
            new Application { Id = 9, Name = "Admin", Description = "System Administration", BaseUrl = "https://admin.yourcompany.com", IconName = "admin_panel_settings", IconColor = "#616161", DisplayOrder = 9, CreatedAt = seedDate }
        );
    }
}
