using System.Security.Claims;
using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.Models.Identity;
using CoreAppwithSSO.API.Services;
using CoreAppwithSSO.API.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Controllers;

/// <summary>
/// Tenant-admin management of machine-to-machine API keys.
/// Keys are scoped to the caller's tenant; the secret is shown exactly once on create.
/// </summary>
[ApiController]
[Route("api/tenant/api-keys")]
[Authorize(Policy = "TenantAdminOnly", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ApiKeysController : ControllerBase
{
    private readonly IdentityDbContext _identity;
    private readonly IApiKeyService _keys;
    private readonly IEncryptionService _encryption;

    public ApiKeysController(IdentityDbContext identity, IApiKeyService keys, IEncryptionService encryption)
    {
        _identity = identity;
        _keys = keys;
        _encryption = encryption;
    }

    [HttpGet]
    public async Task<ActionResult<List<ApiKeySummaryDto>>> List()
    {
        var tenantId = CurrentTenantId();
        var rows = await _identity.ApiKeys
            .Where(k => k.TenantId == tenantId)
            .OrderByDescending(k => k.CreatedDate)
            .ToListAsync();

        return Ok(rows.Select(k => Map(k)).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<CreateApiKeyResponseDto>> Create(CreateApiKeyDto dto)
    {
        var tenantId = CurrentTenantId();

        // Only one active key per tenant — caller must revoke the existing one first.
        var hasActive = await _identity.ApiKeys
            .AnyAsync(k => k.TenantId == tenantId && k.RevokedAt == null);
        if (hasActive)
            return Conflict(new { message = "An active API key already exists for this tenant. Revoke it before creating a new one." });

        var (prefix, full, hash) = _keys.Generate();

        var record = new ApiKey
        {
            TenantId = tenantId,
            Name = dto.Name,
            KeyPrefix = prefix,
            KeyHash = hash,
            SecretEncrypted = _encryption.EncryptApiKey(full),
            Scopes = dto.Scopes != null && dto.Scopes.Count > 0
                ? string.Join(',', dto.Scopes.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)))
                : null,
            ExpiresAt = dto.ExpiresAt,
            CreatedBy = CurrentUserEmail()
        };

        _identity.ApiKeys.Add(record);
        await _identity.SaveChangesAsync();

        var resp = Map(record, includeFullKey: full);
        return CreatedAtAction(nameof(List), null, resp);
    }

    /// <summary>Revokes a key (soft delete). Revoked keys can no longer authenticate.</summary>
    [HttpPost("{id:int}/revoke")]
    public async Task<IActionResult> Revoke(int id)
    {
        var tenantId = CurrentTenantId();
        var record = await _identity.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == id && k.TenantId == tenantId);
        if (record == null) return NotFound();

        if (record.RevokedAt == null)
        {
            record.RevokedAt = DateTime.UtcNow;
            await _identity.SaveChangesAsync();
        }

        return Ok(Map(record));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tenantId = CurrentTenantId();
        var record = await _identity.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == id && k.TenantId == tenantId);
        if (record == null) return NotFound();

        _identity.ApiKeys.Remove(record);
        await _identity.SaveChangesAsync();
        return NoContent();
    }

    private static ApiKeySummaryDto Map(ApiKey k, string? includeFullKey = null)
    {
        var dto = new ApiKeySummaryDto
        {
            Id = k.Id,
            Name = k.Name,
            KeyPrefix = k.KeyPrefix,
            Scopes = string.IsNullOrWhiteSpace(k.Scopes)
                ? new List<string>()
                : k.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            CreatedDate = k.CreatedDate,
            CreatedBy = k.CreatedBy,
            ExpiresAt = k.ExpiresAt,
            LastUsedAt = k.LastUsedAt,
            RevokedAt = k.RevokedAt,
            IsActive = k.RevokedAt == null && (k.ExpiresAt == null || k.ExpiresAt.Value > DateTime.UtcNow)
        };

        if (includeFullKey != null)
        {
            return new CreateApiKeyResponseDto
            {
                Id = dto.Id,
                Name = dto.Name,
                KeyPrefix = dto.KeyPrefix,
                Scopes = dto.Scopes,
                CreatedDate = dto.CreatedDate,
                CreatedBy = dto.CreatedBy,
                ExpiresAt = dto.ExpiresAt,
                LastUsedAt = dto.LastUsedAt,
                RevokedAt = dto.RevokedAt,
                IsActive = dto.IsActive,
                FullKey = includeFullKey
            };
        }

        return dto;
    }

    private int CurrentTenantId()
    {
        var raw = User.FindFirstValue(TenantContextMiddleware.TenantIdClaim);
        return int.Parse(raw!);
    }

    private string CurrentUserEmail()
        => User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
}
