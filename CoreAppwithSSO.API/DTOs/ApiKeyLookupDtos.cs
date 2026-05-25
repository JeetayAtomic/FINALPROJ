using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.DTOs;

/// <summary>
/// Response from <c>GET /api/admin/apikey-lookup</c>. <see cref="ApiKey"/> is null
/// when the tenant only has legacy (hash-only) keys; the caller should rotate.
/// </summary>
public class ApiKeyLookupResponseDto
{
    public string ClientCode { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public string? KeyPrefix { get; set; }
    public string? Name { get; set; }
    public string? ApiKey { get; set; }
    public bool RotationRequired { get; set; }
    public string? Message { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class SetMasterSecretDto
{
    /// <summary>The new master secret. Stored as a SHA-256 hash; cannot be recovered later.</summary>
    [Required, MinLength(32)]
    public string Secret { get; set; } = string.Empty;
}
