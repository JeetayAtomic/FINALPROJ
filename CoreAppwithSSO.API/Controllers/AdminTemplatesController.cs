using System.Security.Claims;
using CoreAppwithSSO.API.Authentication;
using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.Models.Identity;
using CoreAppwithSSO.API.Services;
using CoreAppwithSSO.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Controllers;

/// <summary>
/// Super-admin cross-tenant views of JSON templates. Iterates each tenant DB to
/// flatten + lookup; this is fine for a modest tenant count (sequential reads).
/// </summary>
[ApiController]
[Route("api/admin")]
public class AdminTemplatesController : ControllerBase
{
    private readonly IdentityDbContext _identity;
    private readonly ITenantConnectionResolver _resolver;

    public AdminTemplatesController(
        IdentityDbContext identity,
        ITenantConnectionResolver resolver)
    {
        _identity = identity;
        _resolver = resolver;
    }

    /// <summary>Flat list of every JSON template across every active tenant.</summary>
    [HttpGet("templates")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<List<AdminTemplateRowDto>>> GetAllTemplates()
    {
        var tenants = await _identity.Tenants
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var rows = new List<AdminTemplateRowDto>();

        foreach (var tenant in tenants)
        {
            await using var db = OpenTenant(tenant.DbName);
            var templates = await db.JsonTemplates
                .OrderBy(t => t.Name)
                .ToListAsync();

            foreach (var t in templates)
            {
                rows.Add(new AdminTemplateRowDto
                {
                    TenantId = tenant.Id,
                    TenantName = tenant.Name,
                    ClientCode = tenant.ClientCode,
                    TemplateId = t.Id,
                    TemplateName = t.Name,
                    Description = t.Description,
                    Version = t.Version,
                    CreatedAt = t.CreatedAt,
                    CreatedBy = t.CreatedBy,
                    LastUpdatedAt = t.LastUpdatedAt,
                    LastUpdatedBy = t.LastUpdatedBy
                });
            }
        }

        return Ok(rows);
    }

    /// <summary>
    /// Look up a tenant by client code and pull a single named template's full body.
    /// Both lookups are case-insensitive. Accepts either a SuperAdmin JWT or a valid
    /// X-Api-Key; when called with an API key, the key's tenant must match clientCode.
    /// </summary>
    [HttpGet("lookup")]
    [Authorize(Policy = "SuperAdminOrApiKey")]
    public async Task<ActionResult<AdminLookupResponseDto>> Lookup(
        [FromQuery] string clientCode,
        [FromQuery] string templateName)
    {
        if (string.IsNullOrWhiteSpace(clientCode) || string.IsNullOrWhiteSpace(templateName))
            return BadRequest(new { message = "clientCode and templateName are required." });

        var tenant = await _identity.Tenants
            .FirstOrDefaultAsync(t => t.ClientCode.ToLower() == clientCode.ToLower());
        if (tenant == null)
            return NotFound(new { message = $"No tenant with client code '{clientCode}'." });

        // API-key callers are restricted to their own tenant.
        if (User.IsInRole(ApiKeyAuthenticationHandler.ApiKeyRole))
        {
            var raw = User.FindFirstValue(TenantContextMiddleware.TenantIdClaim);
            if (!int.TryParse(raw, out var apiKeyTenantId) || apiKeyTenantId != tenant.Id)
                return Forbid();
        }

        await using var db = OpenTenant(tenant.DbName);
        var template = await db.JsonTemplates
            .FirstOrDefaultAsync(t => t.Name.ToLower() == templateName.ToLower());
        if (template == null)
            return NotFound(new { message = $"No template named '{templateName}' in tenant '{tenant.Name}'." });

        return Ok(new AdminLookupResponseDto
        {
            Tenant = MapTenant(tenant),
            Template = new JsonTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                JsonContent = template.JsonContent,
                Version = template.Version,
                CreatedAt = template.CreatedAt,
                CreatedBy = template.CreatedBy,
                LastUpdatedAt = template.LastUpdatedAt,
                LastUpdatedBy = template.LastUpdatedBy
            }
        });
    }

    private TenantDbContext OpenTenant(string dbName)
    {
        var connStr = _resolver.Resolve(dbName);
        var opts = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(connStr)
            .Options;
        return new TenantDbContext(opts);
    }

    private TenantDto MapTenant(Tenant t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Slug = t.Slug,
        DbName = t.DbName,
        ClientCode = t.ClientCode,
        TemplateId = t.TemplateId,
        OrganizationId = t.OrganizationId,
        ClientFolderPath = t.ClientFolderPath,
        RawFolderPath = t.RawFolderPath,
        ProcessedFolderPath = t.ProcessedFolderPath,
        ErrorFolderPath = t.ErrorFolderPath,
        PublishFolderPath = t.PublishFolderPath,
        JsonFolderPath = t.JsonFolderPath,
        IsActive = t.IsActive,
        CreatedDate = t.CreatedDate,
        CreatedBy = t.CreatedBy,
        LastUpdatedDate = t.LastUpdatedDate,
        LastUpdatedBy = t.LastUpdatedBy
    };
}
