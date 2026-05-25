using System.Security.Claims;
using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.Models.Sso;
using CoreAppwithSSO.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IdentityDbContext _identity;
    private readonly SsoDbContext _sso;
    private readonly IJwtService _jwt;

    public AuthController(IdentityDbContext identity, SsoDbContext sso, IJwtService jwt)
    {
        _identity = identity;
        _sso = sso;
        _jwt = jwt;
    }

    /// <summary>
    /// Credential check. Branches:
    ///  - Super admin → returns tenant-less super-admin JWT immediately.
    ///  - Regular user → returns interim token + tenant list; client calls /auth/select-tenant.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
    {
        var user = await _identity.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        if (user.IsSuperAdmin)
        {
            return Ok(new LoginResponseDto
            {
                IsSuperAdmin = true,
                Token = _jwt.GenerateSuperAdminToken(user),
                FullName = user.FullName,
                Email = user.Email,
                Tenants = new List<TenantSummaryDto>()
            });
        }

        var tenants = await _identity.UserTenants
            .Where(ut => ut.UserId == user.Id && ut.Tenant.IsActive)
            .Select(ut => new TenantSummaryDto
            {
                TenantId = ut.TenantId,
                Slug = ut.Tenant.Slug,
                Name = ut.Tenant.Name,
                Role = ut.Role,
                LogoUrl = ut.Tenant.LogoUrl
            })
            .ToListAsync();

        if (tenants.Count == 0)
            return Unauthorized(new { message = "User has no active tenant memberships." });

        return Ok(new LoginResponseDto
        {
            IsSuperAdmin = false,
            InterimToken = _jwt.GenerateInterimToken(user),
            FullName = user.FullName,
            Email = user.Email,
            Tenants = tenants
        });
    }

    /// <summary>Exchange an interim token + chosen tenant for a tenant-scoped JWT.</summary>
    [HttpPost("select-tenant")]
    [Authorize(Policy = "InterimAuth")]
    public async Task<ActionResult<AuthResponseDto>> SelectTenant(SelectTenantDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var membership = await _identity.UserTenants
            .Include(ut => ut.User)
            .Include(ut => ut.Tenant)
            .FirstOrDefaultAsync(ut => ut.UserId == userId
                                       && ut.TenantId == dto.TenantId
                                       && ut.Tenant.IsActive
                                       && ut.User.IsActive);

        if (membership == null) return Forbid();

        var token = _jwt.GenerateTenantToken(membership.User, membership.TenantId, membership.Role);

        // Create a server-side session row so the dashboard participates in cross-app logout.
        var session = new UserSession
        {
            UserId = membership.UserId,
            TenantId = membership.TenantId,
            ApplicationId = 0,        // 0 = dashboard (non-SSO) session
            SsoTokenId = null
        };
        _sso.UserSessions.Add(session);
        await _sso.SaveChangesAsync();

        return Ok(new AuthResponseDto
        {
            Token = token,
            FullName = membership.User.FullName,
            Email = membership.User.Email,
            TenantId = membership.TenantId,
            TenantName = membership.Tenant.Name,
            Role = membership.Role,
            TenantLogoUrl = membership.Tenant.LogoUrl,
            SessionId = session.Id
        });
    }
}
