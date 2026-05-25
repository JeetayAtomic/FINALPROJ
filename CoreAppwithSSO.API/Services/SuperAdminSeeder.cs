using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.Models.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Services;

/// <summary>
/// Ensures a super admin account exists at startup, using credentials from
/// appsettings.json (SuperAdmin:Email + SuperAdmin:Password + SuperAdmin:FullName).
/// - If no user with that email exists, creates one.
/// - If the user exists, promotes them to IsSuperAdmin=true (password untouched).
/// </summary>
public class SuperAdminSeeder
{
    private readonly IdentityDbContext _identity;
    private readonly IConfiguration _config;
    private readonly ILogger<SuperAdminSeeder> _logger;

    public SuperAdminSeeder(IdentityDbContext identity, IConfiguration config, ILogger<SuperAdminSeeder> logger)
    {
        _identity = identity;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var email = _config["SuperAdmin:Email"];
        var password = _config["SuperAdmin:Password"];
        var fullName = _config["SuperAdmin:FullName"] ?? "Super Admin";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("SuperAdmin:Email or SuperAdmin:Password not configured. Skipping super admin seed.");
            return;
        }

        var existing = await _identity.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existing == null)
        {
            _identity.Users.Add(new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsSuperAdmin = true,
                IsActive = true
            });
            await _identity.SaveChangesAsync();
            _logger.LogInformation("Created super admin {Email}.", email);
        }
        else if (!existing.IsSuperAdmin)
        {
            existing.IsSuperAdmin = true;
            await _identity.SaveChangesAsync();
            _logger.LogInformation("Promoted existing user {Email} to super admin.", email);
        }
    }
}
