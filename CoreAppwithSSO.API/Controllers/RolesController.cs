using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.Models.Tenant;
using CoreAppwithSSO.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Controllers;

/// <summary>
/// Tenant-admin-only role management within the caller's current tenant.
/// </summary>
[ApiController]
[Route("api/tenant/roles")]
[Authorize(Policy = "TenantAdminOnly")]
public class RolesController : ControllerBase
{
    private readonly TenantDbContext _tenant;
    private readonly IdentityDbContext _identity;
    private readonly SsoDbContext _sso;
    private readonly ITenantContext _tenantContext;

    public RolesController(
        TenantDbContext tenant,
        IdentityDbContext identity,
        SsoDbContext sso,
        ITenantContext tenantContext)
    {
        _tenant = tenant;
        _identity = identity;
        _sso = sso;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoleDto>>> List()
    {
        var tenantId = RequireTenantId();

        var roles = await _tenant.Roles
            .Include(r => r.RoleApplications)
            .OrderBy(r => r.IsSystem ? 0 : 1)
            .ThenBy(r => r.Name)
            .ToListAsync();

        // Member counts come from IdentityDb (by role name).
        var roleNames = roles.Select(r => r.Name).ToList();
        var memberCounts = await _identity.UserTenants
            .Where(ut => ut.TenantId == tenantId && roleNames.Contains(ut.Role))
            .GroupBy(ut => ut.Role)
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToListAsync();
        var byName = memberCounts.ToDictionary(x => x.Role, x => x.Count);

        return Ok(roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsSystem = r.IsSystem,
            CreatedAt = r.CreatedAt,
            ApplicationIds = r.RoleApplications.Select(ra => ra.ApplicationId).ToList(),
            MemberCount = byName.TryGetValue(r.Name, out var c) ? c : 0
        }).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoleDto>> Get(int id)
    {
        var role = await _tenant.Roles
            .Include(r => r.RoleApplications)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (role == null) return NotFound();

        return Ok(new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            CreatedAt = role.CreatedAt,
            ApplicationIds = role.RoleApplications.Select(ra => ra.ApplicationId).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create(CreateRoleDto dto)
    {
        var tenantId = RequireTenantId();

        if (await _tenant.Roles.AnyAsync(r => r.Name == dto.Name))
            return BadRequest(new { message = "A role with this name already exists." });

        await ValidateApplicationIds(tenantId, dto.ApplicationIds);

        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description,
            IsSystem = false
        };
        _tenant.Roles.Add(role);
        await _tenant.SaveChangesAsync();

        foreach (var appId in dto.ApplicationIds.Distinct())
        {
            _tenant.RoleApplications.Add(new RoleApplication { RoleId = role.Id, ApplicationId = appId });
        }
        await _tenant.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = role.Id }, await BuildDto(role.Id));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoleDto>> Update(int id, UpdateRoleDto dto)
    {
        var tenantId = RequireTenantId();

        var role = await _tenant.Roles
            .Include(r => r.RoleApplications)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (role == null) return NotFound();

        // Renaming a system role would orphan UserTenants.Role references in IdentityDb.
        if (role.IsSystem && !string.Equals(role.Name, dto.Name, StringComparison.Ordinal))
            return BadRequest(new { message = "System roles cannot be renamed." });

        // Rename must propagate to IdentityDb.UserTenants.Role.
        if (!string.Equals(role.Name, dto.Name, StringComparison.Ordinal))
        {
            if (await _tenant.Roles.AnyAsync(r => r.Id != id && r.Name == dto.Name))
                return BadRequest(new { message = "A role with this name already exists." });

            var memberships = await _identity.UserTenants
                .Where(ut => ut.TenantId == tenantId && ut.Role == role.Name)
                .ToListAsync();
            foreach (var m in memberships) m.Role = dto.Name;
            await _identity.SaveChangesAsync();

            role.Name = dto.Name;
        }
        role.Description = dto.Description;

        await ValidateApplicationIds(tenantId, dto.ApplicationIds);

        // Replace the role-application set.
        _tenant.RoleApplications.RemoveRange(role.RoleApplications);
        foreach (var appId in dto.ApplicationIds.Distinct())
        {
            _tenant.RoleApplications.Add(new RoleApplication { RoleId = role.Id, ApplicationId = appId });
        }
        await _tenant.SaveChangesAsync();

        return Ok(await BuildDto(role.Id));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tenantId = RequireTenantId();

        var role = await _tenant.Roles.FindAsync(id);
        if (role == null) return NotFound();
        if (role.IsSystem)
            return BadRequest(new { message = "System roles cannot be deleted." });

        var inUse = await _identity.UserTenants
            .AnyAsync(ut => ut.TenantId == tenantId && ut.Role == role.Name);
        if (inUse)
            return BadRequest(new { message = "Role is assigned to one or more users. Reassign them first." });

        _tenant.Roles.Remove(role);
        await _tenant.SaveChangesAsync();
        return NoContent();
    }

    private async Task<RoleDto> BuildDto(int roleId)
    {
        var role = await _tenant.Roles
            .Include(r => r.RoleApplications)
            .FirstAsync(r => r.Id == roleId);
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            CreatedAt = role.CreatedAt,
            ApplicationIds = role.RoleApplications.Select(ra => ra.ApplicationId).ToList()
        };
    }

    /// <summary>Ensures every requested app is (a) globally active and (b) subscribed to this tenant.</summary>
    private async Task ValidateApplicationIds(int tenantId, List<int> ids)
    {
        if (ids == null || ids.Count == 0) return;

        var distinct = ids.Distinct().ToList();
        var subscribed = await _sso.TenantApplications
            .Where(ta => ta.TenantId == tenantId && ta.IsActive && distinct.Contains(ta.ApplicationId))
            .Select(ta => ta.ApplicationId)
            .ToListAsync();

        var active = await _sso.Applications
            .Where(a => a.IsActive && subscribed.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync();

        var missing = distinct.Except(active).ToList();
        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"Application Id(s) {string.Join(",", missing)} are not available to this tenant.");
    }

    private int RequireTenantId() =>
        _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context not set.");
}
