using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Controllers;

/// <summary>
/// Tenant-admin endpoints to view subscribed apps and toggle the per-tenant IsActive flag.
/// Tenant admins cannot subscribe to new apps — that requires super-admin.
/// </summary>
[ApiController]
[Route("api/tenant/applications")]
[Authorize(Policy = "TenantAdminOnly")]
public class TenantApplicationsController : ControllerBase
{
    private readonly SsoDbContext _sso;
    private readonly ITenantContext _tenantContext;

    public TenantApplicationsController(SsoDbContext sso, ITenantContext tenantContext)
    {
        _sso = sso;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<TenantApplicationDto>>> List()
    {
        var tenantId = RequireTenantId();

        var subs = await _sso.TenantApplications
            .Where(ta => ta.TenantId == tenantId)
            .Include(ta => ta.Application)
            .OrderBy(ta => ta.Application.DisplayOrder)
            .ToListAsync();

        return Ok(subs.Select(s => new TenantApplicationDto
        {
            ApplicationId = s.ApplicationId,
            Name = s.Application.Name,
            Description = s.Application.Description,
            IconName = s.Application.IconName,
            IconColor = s.Application.IconColor,
            DisplayOrder = s.Application.DisplayOrder,
            ApplicationActive = s.Application.IsActive,
            IsSubscribed = true,
            SubscriptionActive = s.IsActive,
            AssignedAt = s.AssignedAt
        }).ToList());
    }

    [HttpPut("{applicationId:int}")]
    public async Task<ActionResult<TenantApplicationDto>> Toggle(
        int applicationId,
        TenantApplicationWriteDto dto)
    {
        var tenantId = RequireTenantId();

        var sub = await _sso.TenantApplications
            .Include(ta => ta.Application)
            .FirstOrDefaultAsync(ta => ta.TenantId == tenantId && ta.ApplicationId == applicationId);

        if (sub == null)
            return NotFound(new { message = "This tenant is not subscribed to that application. Ask a super-admin to subscribe first." });

        sub.IsActive = dto.IsActive;
        await _sso.SaveChangesAsync();

        return Ok(new TenantApplicationDto
        {
            ApplicationId = sub.ApplicationId,
            Name = sub.Application.Name,
            Description = sub.Application.Description,
            IconName = sub.Application.IconName,
            IconColor = sub.Application.IconColor,
            DisplayOrder = sub.Application.DisplayOrder,
            ApplicationActive = sub.Application.IsActive,
            IsSubscribed = true,
            SubscriptionActive = sub.IsActive,
            AssignedAt = sub.AssignedAt
        });
    }

    private int RequireTenantId()
        => _tenantContext.TenantId
            ?? throw new InvalidOperationException("Tenant context not set.");
}
