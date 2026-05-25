using System.Security.Claims;
using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly SsoDbContext _sso;
    private readonly TenantDbContext _tenant;
    private readonly ITenantContext _tenantContext;

    public ApplicationsController(SsoDbContext sso, TenantDbContext tenant, ITenantContext tenantContext)
    {
        _sso = sso;
        _tenant = tenant;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Apps visible to the current user in the current tenant.
    /// Access chain: user's tenant role → Role → RoleApplications → intersect with the
    /// tenant's subscribed apps → intersect with globally active apps.
    /// </summary>
    [HttpGet("my-apps")]
    public async Task<ActionResult<List<ApplicationDto>>> GetMyApplications()
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("Tenant context not set.");
        var roleName = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(roleName))
            return Ok(new List<ApplicationDto>());

        var roleAppIds = await _tenant.Roles
            .Where(r => r.Name == roleName)
            .SelectMany(r => r.RoleApplications)
            .Select(ra => ra.ApplicationId)
            .ToListAsync();

        if (roleAppIds.Count == 0)
            return Ok(new List<ApplicationDto>());

        var subscribedIds = await _sso.TenantApplications
            .Where(ta => ta.TenantId == tenantId && ta.IsActive)
            .Select(ta => ta.ApplicationId)
            .ToListAsync();

        var visibleIds = roleAppIds.Intersect(subscribedIds).ToList();
        if (visibleIds.Count == 0)
            return Ok(new List<ApplicationDto>());

        var apps = await _sso.Applications
            .Where(a => visibleIds.Contains(a.Id) && a.IsActive)
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new ApplicationDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                BaseUrl = a.BaseUrl,
                IconName = a.IconName,
                IconColor = a.IconColor,
                DisplayOrder = a.DisplayOrder
            })
            .ToListAsync();

        return Ok(apps);
    }

    /// <summary>Apps subscribed to the current tenant (used by the role editor).</summary>
    [HttpGet("tenant-catalog")]
    public async Task<ActionResult<List<ApplicationDto>>> GetTenantCatalog()
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("Tenant context not set.");

        var subscribedIds = await _sso.TenantApplications
            .Where(ta => ta.TenantId == tenantId && ta.IsActive)
            .Select(ta => ta.ApplicationId)
            .ToListAsync();

        var apps = await _sso.Applications
            .Where(a => subscribedIds.Contains(a.Id) && a.IsActive)
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new ApplicationDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                BaseUrl = a.BaseUrl,
                IconName = a.IconName,
                IconColor = a.IconColor,
                DisplayOrder = a.DisplayOrder
            })
            .ToListAsync();

        return Ok(apps);
    }
}
