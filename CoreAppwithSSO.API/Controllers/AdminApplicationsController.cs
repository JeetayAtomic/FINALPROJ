using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.Models.Sso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Controllers;

/// <summary>
/// Super-admin CRUD over the global Application catalog and per-tenant subscriptions.
/// </summary>
[ApiController]
[Authorize(Policy = "SuperAdminOnly")]
public class AdminApplicationsController : ControllerBase
{
    private readonly SsoDbContext _sso;
    private readonly IdentityDbContext _identity;

    public AdminApplicationsController(SsoDbContext sso, IdentityDbContext identity)
    {
        _sso = sso;
        _identity = identity;
    }

    // ---------- Application catalog CRUD ----------

    [HttpGet("api/admin/applications")]
    public async Task<ActionResult<List<ApplicationAdminDto>>> List()
    {
        var apps = await _sso.Applications
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();
        return Ok(apps.Select(Map).ToList());
    }

    [HttpGet("api/admin/applications/{id:int}")]
    public async Task<ActionResult<ApplicationAdminDto>> Get(int id)
    {
        var app = await _sso.Applications.FindAsync(id);
        if (app == null) return NotFound();
        return Ok(Map(app));
    }

    [HttpPost("api/admin/applications")]
    public async Task<ActionResult<ApplicationAdminDto>> Create(CreateApplicationDto dto)
    {
        if (await _sso.Applications.AnyAsync(a => a.Name == dto.Name))
            return BadRequest(new { message = "An application with this name already exists." });

        var app = new Application
        {
            Name = dto.Name,
            Description = dto.Description,
            BaseUrl = dto.BaseUrl,
            IconName = string.IsNullOrWhiteSpace(dto.IconName) ? "apps" : dto.IconName,
            IconColor = string.IsNullOrWhiteSpace(dto.IconColor) ? "#4285F4" : dto.IconColor,
            DisplayOrder = dto.DisplayOrder,
            IsActive = true
        };
        _sso.Applications.Add(app);
        await _sso.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = app.Id }, Map(app));
    }

    [HttpPut("api/admin/applications/{id:int}")]
    public async Task<ActionResult<ApplicationAdminDto>> Update(int id, UpdateApplicationDto dto)
    {
        var app = await _sso.Applications.FindAsync(id);
        if (app == null) return NotFound();

        if (!string.Equals(app.Name, dto.Name, StringComparison.OrdinalIgnoreCase)
            && await _sso.Applications.AnyAsync(a => a.Id != id && a.Name == dto.Name))
        {
            return BadRequest(new { message = "An application with this name already exists." });
        }

        app.Name = dto.Name;
        app.Description = dto.Description;
        app.BaseUrl = dto.BaseUrl;
        app.IconName = string.IsNullOrWhiteSpace(dto.IconName) ? "apps" : dto.IconName;
        app.IconColor = string.IsNullOrWhiteSpace(dto.IconColor) ? "#4285F4" : dto.IconColor;
        app.DisplayOrder = dto.DisplayOrder;
        app.IsActive = dto.IsActive;

        await _sso.SaveChangesAsync();
        return Ok(Map(app));
    }

    [HttpDelete("api/admin/applications/{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var app = await _sso.Applications.FindAsync(id);
        if (app == null) return NotFound();

        app.IsActive = false;
        await _sso.SaveChangesAsync();
        return NoContent();
    }

    // ---------- Per-tenant subscriptions ----------

    /// <summary>
    /// Returns every catalog app with this tenant's subscription state, so the UI
    /// can render a single list with a toggle per row.
    /// </summary>
    [HttpGet("api/admin/tenants/{tenantId:int}/applications")]
    public async Task<ActionResult<List<TenantApplicationDto>>> ListTenantApps(int tenantId)
    {
        var tenantExists = await _identity.Tenants.AnyAsync(t => t.Id == tenantId);
        if (!tenantExists) return NotFound();

        var subs = await _sso.TenantApplications
            .Where(ta => ta.TenantId == tenantId)
            .ToDictionaryAsync(ta => ta.ApplicationId);

        var apps = await _sso.Applications
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();

        var rows = apps.Select(a => new TenantApplicationDto
        {
            ApplicationId = a.Id,
            Name = a.Name,
            Description = a.Description,
            IconName = a.IconName,
            IconColor = a.IconColor,
            DisplayOrder = a.DisplayOrder,
            ApplicationActive = a.IsActive,
            IsSubscribed = subs.ContainsKey(a.Id),
            SubscriptionActive = subs.TryGetValue(a.Id, out var s) && s.IsActive,
            AssignedAt = subs.TryGetValue(a.Id, out var s2) ? s2.AssignedAt : (DateTime?)null
        }).ToList();

        return Ok(rows);
    }

    /// <summary>Subscribe a tenant to an application, or update the IsActive flag if already subscribed.</summary>
    [HttpPut("api/admin/tenants/{tenantId:int}/applications/{applicationId:int}")]
    public async Task<ActionResult<TenantApplicationDto>> UpsertTenantApp(
        int tenantId,
        int applicationId,
        TenantApplicationWriteDto dto)
    {
        var tenantExists = await _identity.Tenants.AnyAsync(t => t.Id == tenantId);
        if (!tenantExists) return NotFound(new { message = "Tenant not found." });

        var app = await _sso.Applications.FindAsync(applicationId);
        if (app == null) return NotFound(new { message = "Application not found." });

        var sub = await _sso.TenantApplications
            .FirstOrDefaultAsync(ta => ta.TenantId == tenantId && ta.ApplicationId == applicationId);

        if (sub == null)
        {
            sub = new TenantApplication
            {
                TenantId = tenantId,
                ApplicationId = applicationId,
                IsActive = dto.IsActive
            };
            _sso.TenantApplications.Add(sub);
        }
        else
        {
            sub.IsActive = dto.IsActive;
        }

        await _sso.SaveChangesAsync();

        return Ok(new TenantApplicationDto
        {
            ApplicationId = app.Id,
            Name = app.Name,
            Description = app.Description,
            IconName = app.IconName,
            IconColor = app.IconColor,
            DisplayOrder = app.DisplayOrder,
            ApplicationActive = app.IsActive,
            IsSubscribed = true,
            SubscriptionActive = sub.IsActive,
            AssignedAt = sub.AssignedAt
        });
    }

    /// <summary>Remove a tenant's subscription entirely.</summary>
    [HttpDelete("api/admin/tenants/{tenantId:int}/applications/{applicationId:int}")]
    public async Task<IActionResult> RemoveTenantApp(int tenantId, int applicationId)
    {
        var sub = await _sso.TenantApplications
            .FirstOrDefaultAsync(ta => ta.TenantId == tenantId && ta.ApplicationId == applicationId);
        if (sub == null) return NotFound();

        _sso.TenantApplications.Remove(sub);
        await _sso.SaveChangesAsync();
        return NoContent();
    }

    private static ApplicationAdminDto Map(Application a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Description = a.Description,
        BaseUrl = a.BaseUrl,
        IconName = a.IconName,
        IconColor = a.IconColor,
        DisplayOrder = a.DisplayOrder,
        IsActive = a.IsActive,
        CreatedAt = a.CreatedAt
    };
}
