using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoreAppwithSSO.API.Models.Identity;
using CoreAppwithSSO.API.Tenancy;
using Microsoft.IdentityModel.Tokens;

namespace CoreAppwithSSO.API.Services;

public interface IJwtService
{
    /// <summary>
    /// Interim token issued at login when the user has not yet picked a tenant.
    /// Has 'sub' and 'token_use=interim' but no 'tenant_id'. Short-lived.
    /// </summary>
    string GenerateInterimToken(User user);

    /// <summary>Tenant-scoped token. Has 'sub', 'tenant_id', and the per-tenant role.</summary>
    string GenerateTenantToken(User user, int tenantId, string role);

    /// <summary>Super-admin token. Has 'sub' and 'is_super_admin=true'; no 'tenant_id'.</summary>
    string GenerateSuperAdminToken(User user);
}

public class JwtService : IJwtService
{
    public const string TokenUseClaim = "token_use";
    public const string TokenUseInterim = "interim";
    public const string TokenUseTenant = "tenant";
    public const string TokenUseSuperAdmin = "super_admin";
    public const string IsSuperAdminClaim = "is_super_admin";

    public const string RoleSuperAdmin = "SuperAdmin";

    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateInterimToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(TokenUseClaim, TokenUseInterim)
        };
        return Build(claims, TimeSpan.FromMinutes(10));
    }

    public string GenerateTenantToken(User user, int tenantId, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role),
            new(TenantContextMiddleware.TenantIdClaim, tenantId.ToString()),
            new(TokenUseClaim, TokenUseTenant)
        };
        return Build(claims, TimeSpan.FromHours(8));
    }

    public string GenerateSuperAdminToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, RoleSuperAdmin),
            new(IsSuperAdminClaim, "true"),
            new(TokenUseClaim, TokenUseSuperAdmin)
        };
        return Build(claims, TimeSpan.FromHours(8));
    }

    private string Build(IEnumerable<Claim> claims, TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
