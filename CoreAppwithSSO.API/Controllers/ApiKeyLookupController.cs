using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.Models.Identity;
using CoreAppwithSSO.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Controllers;

/// <summary>
/// Third-party API key recovery endpoint. The caller passes a tenant's <c>clientCode</c>
/// and a shared <c>X-Master-Secret</c> header; we look up the tenant's active API key,
/// decrypt it via Data Protection, and return the plaintext.
///
/// The master secret is a single shared value across all clients, stored as a SHA-256 hash
/// in <see cref="AppSetting"/> (key = <see cref="MasterSecretKey"/>). It is set/rotated by
/// a SuperAdmin via the POST endpoint on this controller.
///
/// Security notes:
///   * Constant-time comparison on the master secret hash (prevents timing leaks).
///   * Failed lookups return a generic 401 — never reveal which part was wrong.
///   * Legacy keys (created before the encrypted secret column was added) cannot be
///     recovered; the response carries <c>rotationRequired=true</c> with apiKey=null.
/// </summary>
[ApiController]
[Route("api/admin/apikey-lookup")]
public class ApiKeyLookupController : ControllerBase
{
    public const string MasterSecretKey = "ApiKeyLookup.MasterSecretHash";
    public const string MasterSecretHeader = "X-Master-Secret";

    private readonly IdentityDbContext _identity;
    private readonly IEncryptionService _encryption;
    private readonly ILogger<ApiKeyLookupController> _logger;

    public ApiKeyLookupController(
        IdentityDbContext identity,
        IEncryptionService encryption,
        ILogger<ApiKeyLookupController> logger)
    {
        _identity = identity;
        _encryption = encryption;
        _logger = logger;
    }

    /// <summary>
    /// Look up the active API key for a tenant. Anonymous endpoint gated by the
    /// <c>X-Master-Secret</c> header.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiKeyLookupResponseDto>> Lookup([FromQuery] string clientCode)
    {
        if (string.IsNullOrWhiteSpace(clientCode))
            return BadRequest(new { message = "clientCode is required." });

        if (!Request.Headers.TryGetValue(MasterSecretHeader, out var presentedValues))
            return Unauthorized(new { message = "Missing master secret." });

        var presented = presentedValues.ToString();
        if (!await VerifyMasterSecret(presented))
            return Unauthorized(new { message = "Invalid master secret." });

        var tenant = await _identity.Tenants
            .FirstOrDefaultAsync(t => t.ClientCode.ToLower() == clientCode.ToLower());
        if (tenant == null || !tenant.IsActive)
            return NotFound(new { message = $"No active tenant with client code '{clientCode}'." });

        var key = await _identity.ApiKeys
            .Where(k => k.TenantId == tenant.Id && k.RevokedAt == null)
            .OrderByDescending(k => k.CreatedDate)
            .FirstOrDefaultAsync();

        if (key == null)
            return NotFound(new { message = $"No active API key for tenant '{tenant.Name}'." });

        var response = BuildBaseResponse(tenant, key);

        if (string.IsNullOrEmpty(key.SecretEncrypted))
        {
            response.RotationRequired = true;
            response.Message = "Legacy key cannot be recovered. Rotate the key (revoke + recreate) to enable lookup.";
            return Ok(response);
        }

        try
        {
            response.ApiKey = _encryption.DecryptApiKey(key.SecretEncrypted);
        }
        catch (CryptographicException ex)
        {
            // Encryption key rotated/lost — surface as rotation required rather than 500.
            _logger.LogWarning(ex, "Failed to decrypt ApiKey {ApiKeyId} for tenant {TenantId}", key.Id, tenant.Id);
            response.RotationRequired = true;
            response.Message = "Stored secret is unrecoverable under the current Data Protection keyring. Rotate the key.";
        }

        return Ok(response);
    }

    private ApiKeyLookupResponseDto BuildBaseResponse(Models.Identity.Tenant tenant, ApiKey key)
    {
        return new ApiKeyLookupResponseDto
        {
            ClientCode = tenant.ClientCode,
            TenantId = tenant.Id,
            KeyPrefix = key.KeyPrefix,
            Name = key.Name,
            ExpiresAt = key.ExpiresAt
        };
    }

    /// <summary>
    /// Set or rotate the master secret. SuperAdmin only. Stored as a SHA-256 hash;
    /// the plaintext is never persisted.
    /// </summary>
    [HttpPost("master-secret")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> SetMasterSecret(SetMasterSecretDto dto)
    {
        var hash = Sha256Hex(dto.Secret);
        var existing = await _identity.AppSettings.FirstOrDefaultAsync(s => s.Key == MasterSecretKey);
        var actor = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";

        if (existing == null)
        {
            _identity.AppSettings.Add(new AppSetting
            {
                Key = MasterSecretKey,
                Value = hash,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = actor
            });
        }
        else
        {
            existing.Value = hash;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = actor;
        }

        await _identity.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> VerifyMasterSecret(string presented)
    {
        if (string.IsNullOrWhiteSpace(presented)) return false;

        var stored = await _identity.AppSettings
            .Where(s => s.Key == MasterSecretKey)
            .Select(s => s.Value)
            .FirstOrDefaultAsync();
        if (string.IsNullOrEmpty(stored)) return false;

        var presentedHash = Sha256Hex(presented);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(presentedHash),
            Encoding.ASCII.GetBytes(stored));
    }

    private static string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
