using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.Models.Identity;
using CoreAppwithSSO.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Controllers;

/// <summary>
/// Tenant-admin-only user management. Operates within the caller's current tenant
/// (derived from the JWT 'tenant_id' claim).
/// </summary>
[ApiController]
[Route("api/tenant/users")]
[Authorize(Policy = "TenantAdminOnly")]
public class TenantUsersController : ControllerBase
{
    private readonly IdentityDbContext _identity;
    private readonly TenantDbContext _tenant;
    private readonly ITenantContext _tenantContext;

    public TenantUsersController(
        IdentityDbContext identity,
        TenantDbContext tenant,
        ITenantContext tenantContext)
    {
        _identity = identity;
        _tenant = tenant;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<TenantUserDto>>> List()
    {
        var tenantId = RequireTenantId();

        var rows = await _identity.UserTenants
            .Where(ut => ut.TenantId == tenantId)
            .Include(ut => ut.User)
            .OrderBy(ut => ut.User.FullName)
            .Select(ut => new TenantUserDto
            {
                UserId = ut.UserId,
                FullName = ut.User.FullName,
                Email = ut.User.Email,
                RoleName = ut.Role,
                IsActive = ut.User.IsActive,
                MembershipCreatedAt = ut.CreatedAt
            })
            .ToListAsync();

        return Ok(rows);
    }

    [HttpPost]
    public async Task<ActionResult<TenantUserDto>> Create(CreateTenantUserDto dto)
    {
        var tenantId = RequireTenantId();

        // Role must exist in this tenant.
        var role = await _tenant.Roles.FirstOrDefaultAsync(r => r.Name == dto.RoleName);
        if (role == null)
            return BadRequest(new { message = $"Role '{dto.RoleName}' not found in this tenant." });

        // Reuse existing identity user (cross-tenant) or create a new one.
        var user = await _identity.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
        {
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                return BadRequest(new { message = "A password of at least 6 characters is required for new users." });

            user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsActive = true
            };
            _identity.Users.Add(user);
            await _identity.SaveChangesAsync();
        }

        // Block if already a member.
        var already = await _identity.UserTenants
            .AnyAsync(ut => ut.UserId == user.Id && ut.TenantId == tenantId);
        if (already)
            return BadRequest(new { message = "User is already a member of this tenant." });

        _identity.UserTenants.Add(new UserTenant
        {
            UserId = user.Id,
            TenantId = tenantId,
            Role = dto.RoleName
        });
        await _identity.SaveChangesAsync();

        return Ok(new TenantUserDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = dto.RoleName,
            IsActive = user.IsActive,
            MembershipCreatedAt = DateTime.UtcNow
        });
    }

    [HttpPut("{userId:int}")]
    public async Task<ActionResult<TenantUserDto>> Update(int userId, UpdateTenantUserDto dto)
    {
        var tenantId = RequireTenantId();

        var membership = await _identity.UserTenants
            .Include(ut => ut.User)
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TenantId == tenantId);
        if (membership == null) return NotFound();

        // Role must exist in this tenant.
        var role = await _tenant.Roles.FirstOrDefaultAsync(r => r.Name == dto.RoleName);
        if (role == null)
            return BadRequest(new { message = $"Role '{dto.RoleName}' not found in this tenant." });

        membership.User.FullName = dto.FullName;
        membership.User.IsActive = dto.IsActive;
        membership.Role = dto.RoleName;

        await _identity.SaveChangesAsync();

        return Ok(new TenantUserDto
        {
            UserId = membership.UserId,
            FullName = membership.User.FullName,
            Email = membership.User.Email,
            RoleName = membership.Role,
            IsActive = membership.User.IsActive,
            MembershipCreatedAt = membership.CreatedAt
        });
    }

    /// <summary>Removes the user's membership in this tenant. The global identity user is kept.</summary>
    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> RemoveFromTenant(int userId)
    {
        var tenantId = RequireTenantId();

        var membership = await _identity.UserTenants
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TenantId == tenantId);
        if (membership == null) return NotFound();

        _identity.UserTenants.Remove(membership);
        await _identity.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{userId:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int userId, ResetPasswordDto dto)
    {
        var tenantId = RequireTenantId();

        var isMember = await _identity.UserTenants
            .AnyAsync(ut => ut.UserId == userId && ut.TenantId == tenantId);
        if (!isMember) return NotFound();

        var user = await _identity.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        await _identity.SaveChangesAsync();

        return NoContent();
    }

    private int RequireTenantId() =>
        _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context not set.");
}
