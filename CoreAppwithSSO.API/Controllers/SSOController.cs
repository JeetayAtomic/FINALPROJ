using System.Security.Claims;
using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.Models.Sso;
using CoreAppwithSSO.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Controllers;

/// <summary>
/// Tenant-scoped SSO generation — lives on its own controller so it can safely take
/// TenantDbContext (which is resolved from the JWT tenant_id claim).
/// </summary>
[ApiController]
[Route("api/sso")]
public class SSOController : ControllerBase
{
    private readonly SsoDbContext _sso;
    private readonly TenantDbContext _tenant;
    private readonly ITenantContext _tenantContext;

    public SSOController(SsoDbContext sso, TenantDbContext tenant, ITenantContext tenantContext)
    {
        _sso = sso;
        _tenant = tenant;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Generate a one-time SSO token for redirecting the current user (in the current tenant)
    /// to a target application.
    /// </summary>
    [HttpPost("generate/{applicationId}")]
    [Authorize]
    public async Task<ActionResult<SSORedirectDto>> GenerateSSOToken(int applicationId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("Tenant context not set.");

        // 1. Tenant must be subscribed to this application.
        var tenantSubscribed = await _sso.TenantApplications
            .AnyAsync(ta => ta.TenantId == tenantId && ta.ApplicationId == applicationId && ta.IsActive);
        if (!tenantSubscribed) return Forbid();

        // 2. User's role in this tenant must grant access to the application.
        var roleName = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(roleName)) return Forbid();

        var roleGrants = await _tenant.Roles
            .Where(r => r.Name == roleName)
            .AnyAsync(r => r.RoleApplications.Any(ra => ra.ApplicationId == applicationId));
        if (!roleGrants) return Forbid();

        var app = await _sso.Applications.FindAsync(applicationId);
        if (app == null || !app.IsActive)
            return NotFound(new { message = "Application not found." });

        var ssoToken = new SsoToken
        {
            Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
            UserId = userId,
            TenantId = tenantId,
            ApplicationId = applicationId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };

        _sso.SsoTokens.Add(ssoToken);
        await _sso.SaveChangesAsync();

        var redirectUrl = $"{app.BaseUrl.TrimEnd('/')}/sso/callback?token={ssoToken.Token}";

        return Ok(new SSORedirectDto
        {
            RedirectUrl = redirectUrl,
            SsoToken = ssoToken.Token
        });
    }
}

/// <summary>
/// Anonymous SSO validation + session endpoints consumed by target apps.
/// Kept on a separate controller so it doesn't depend on TenantDbContext.
/// </summary>
[ApiController]
[Route("api/sso")]
[AllowAnonymous]
public class SSOValidationController : ControllerBase
{
    private readonly SsoDbContext _sso;
    private readonly IdentityDbContext _identity;

    public SSOValidationController(SsoDbContext sso, IdentityDbContext identity)
    {
        _sso = sso;
        _identity = identity;
    }

    /// <summary>
    /// Validate a one-time SSO token and create a server-side <see cref="UserSession"/>.
    /// Response carries the new SessionId which the target app uses to poll + logout.
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<SSOValidationResponseDto>> ValidateToken(SSOValidationDto dto)
    {
        var ssoToken = await _sso.SsoTokens.FirstOrDefaultAsync(s => s.Token == dto.Token);

        if (ssoToken == null || ssoToken.IsUsed || ssoToken.ExpiresAt < DateTime.UtcNow)
            return Ok(new SSOValidationResponseDto { IsValid = false });

        var user = await _identity.Users.FirstOrDefaultAsync(u => u.Id == ssoToken.UserId && u.IsActive);
        if (user == null) return Ok(new SSOValidationResponseDto { IsValid = false });

        var role = await _identity.UserTenants
            .Where(ut => ut.UserId == ssoToken.UserId && ut.TenantId == ssoToken.TenantId)
            .Select(ut => ut.Role)
            .FirstOrDefaultAsync();
        if (role == null) return Ok(new SSOValidationResponseDto { IsValid = false });

        ssoToken.IsUsed = true;

        var session = new UserSession
        {
            UserId = ssoToken.UserId,
            TenantId = ssoToken.TenantId,
            ApplicationId = ssoToken.ApplicationId,
            SsoTokenId = ssoToken.Id
        };
        _sso.UserSessions.Add(session);
        await _sso.SaveChangesAsync();

        return Ok(new SSOValidationResponseDto
        {
            IsValid = true,
            SessionId = session.Id,
            UserId = user.Id,
            TenantId = ssoToken.TenantId,
            ApplicationId = ssoToken.ApplicationId,
            Email = user.Email,
            FullName = user.FullName,
            Role = role
        });
    }

    /// <summary>
    /// Session status for a specific session id. Used by target apps to poll.
    /// Returns <c>active=false</c> if the session was revoked (e.g., user signed out from another app).
    /// </summary>
    [HttpGet("sessions/{sessionId:int}")]
    public async Task<ActionResult<SessionStatusDto>> GetSession(int sessionId)
    {
        var session = await _sso.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session == null)
            return Ok(new SessionStatusDto { Active = false, SessionId = sessionId });

        if (session.RevokedAt != null)
        {
            return Ok(new SessionStatusDto
            {
                Active = false,
                SessionId = session.Id,
                UserId = session.UserId,
                TenantId = session.TenantId,
                ApplicationId = session.ApplicationId
            });
        }

        session.LastSeenAt = DateTime.UtcNow;
        await _sso.SaveChangesAsync();

        var user = await _identity.Users.FirstOrDefaultAsync(u => u.Id == session.UserId);
        var role = await _identity.UserTenants
            .Where(ut => ut.UserId == session.UserId && ut.TenantId == session.TenantId)
            .Select(ut => ut.Role)
            .FirstOrDefaultAsync();

        if (user == null || role == null)
            return Ok(new SessionStatusDto { Active = false, SessionId = sessionId });

        return Ok(new SessionStatusDto
        {
            Active = true,
            SessionId = session.Id,
            UserId = session.UserId,
            TenantId = session.TenantId,
            ApplicationId = session.ApplicationId,
            Email = user.Email,
            FullName = user.FullName,
            Role = role
        });
    }

    /// <summary>
    /// Logs out the user in every app: revokes every UserSession for the
    /// (UserId, TenantId) tied to the given session id. Idempotent.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutDto dto)
    {
        var session = await _sso.UserSessions.FirstOrDefaultAsync(s => s.Id == dto.SessionId);
        if (session == null) return NoContent();

        var now = DateTime.UtcNow;
        var affected = await _sso.UserSessions
            .Where(s => s.UserId == session.UserId
                        && s.TenantId == session.TenantId
                        && s.RevokedAt == null)
            .ExecuteUpdateAsync(u => u.SetProperty(s => s.RevokedAt, now));

        return NoContent();
    }
}
