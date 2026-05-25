using System.Security.Claims;
using System.Text.Json;
using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.Models.Tenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Controllers;

[ApiController]
[Route("api/tenant/templates")]
[Authorize(Policy = "TenantAdminOnly")]
public class TemplatesController : ControllerBase
{
    private readonly TenantDbContext _tenant;

    public TemplatesController(TenantDbContext tenant)
    {
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<ActionResult<List<JsonTemplateSummaryDto>>> List()
    {
        var list = await _tenant.JsonTemplates
            .OrderBy(t => t.Name)
            .Select(t => new JsonTemplateSummaryDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Version = t.Version,
                CreatedAt = t.CreatedAt,
                CreatedBy = t.CreatedBy,
                LastUpdatedAt = t.LastUpdatedAt,
                LastUpdatedBy = t.LastUpdatedBy
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JsonTemplateDto>> Get(int id)
    {
        var t = await _tenant.JsonTemplates.FindAsync(id);
        if (t == null) return NotFound();

        return Ok(new JsonTemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            JsonContent = t.JsonContent,
            Version = t.Version,
            CreatedAt = t.CreatedAt,
            CreatedBy = t.CreatedBy,
            LastUpdatedAt = t.LastUpdatedAt,
            LastUpdatedBy = t.LastUpdatedBy
        });
    }

    [HttpPost]
    public async Task<ActionResult<JsonTemplateDto>> Create(CreateJsonTemplateDto dto)
    {
        if (!IsValidJson(dto.JsonContent))
            return BadRequest(new { message = "JsonContent is not valid JSON." });

        if (await _tenant.JsonTemplates.AnyAsync(t => t.Name == dto.Name))
            return BadRequest(new { message = "A template with this name already exists." });

        var template = new JsonTemplate
        {
            Name = dto.Name,
            Description = dto.Description,
            JsonContent = dto.JsonContent,
            Version = 1,
            CreatedBy = CurrentUserEmail()
        };

        _tenant.JsonTemplates.Add(template);
        await _tenant.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = template.Id }, MapFull(template));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<JsonTemplateDto>> Update(int id, UpdateJsonTemplateDto dto)
    {
        if (!IsValidJson(dto.JsonContent))
            return BadRequest(new { message = "JsonContent is not valid JSON." });

        var template = await _tenant.JsonTemplates.FindAsync(id);
        if (template == null) return NotFound();

        if (!string.Equals(template.Name, dto.Name, StringComparison.Ordinal)
            && await _tenant.JsonTemplates.AnyAsync(t => t.Id != id && t.Name == dto.Name))
        {
            return BadRequest(new { message = "A template with this name already exists." });
        }

        template.Name = dto.Name;
        template.Description = dto.Description;
        template.JsonContent = dto.JsonContent;
        template.Version++;
        template.LastUpdatedAt = DateTime.UtcNow;
        template.LastUpdatedBy = CurrentUserEmail();

        await _tenant.SaveChangesAsync();

        return Ok(MapFull(template));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var template = await _tenant.JsonTemplates.FindAsync(id);
        if (template == null) return NotFound();

        _tenant.JsonTemplates.Remove(template);
        await _tenant.SaveChangesAsync();

        return NoContent();
    }

    private static bool IsValidJson(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        try
        {
            using var doc = JsonDocument.Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static JsonTemplateDto MapFull(JsonTemplate t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        JsonContent = t.JsonContent,
        Version = t.Version,
        CreatedAt = t.CreatedAt,
        CreatedBy = t.CreatedBy,
        LastUpdatedAt = t.LastUpdatedAt,
        LastUpdatedBy = t.LastUpdatedBy
    };

    private string CurrentUserEmail()
        => User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
}
