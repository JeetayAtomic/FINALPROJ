using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.DTOs;

public class ApiKeySummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateApiKeyDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public List<string>? Scopes { get; set; }

    /// <summary>Optional. If null the key does not expire.</summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Returned once on create. The <see cref="FullKey"/> is the only moment the caller sees the secret.
/// </summary>
public class CreateApiKeyResponseDto : ApiKeySummaryDto
{
    public string FullKey { get; set; } = string.Empty;
}
