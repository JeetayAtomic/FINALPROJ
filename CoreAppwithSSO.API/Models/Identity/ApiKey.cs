using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.Models.Identity;

/// <summary>
/// Tenant-scoped API key for machine-to-machine access.
/// Stored with a short visible prefix + a SHA-256 hash of the full key; the secret is returned to the
/// caller exactly once (on create) and cannot be recovered afterwards.
/// </summary>
public class ApiKey
{
    [Key]
    public int Id { get; set; }

    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>First 8 chars of the secret portion (visible in lists/logs for identification).</summary>
    [Required, MaxLength(16)]
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>Hex-encoded SHA-256 of the full key string.</summary>
    [Required, MaxLength(128)]
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Reversibly-encrypted full key (ciphertext via <see cref="Services.IEncryptionService"/>).
    /// Populated for keys created after the lookup feature shipped; null for legacy keys
    /// (in which case the lookup endpoint returns null + a "rotation required" flag).
    /// </summary>
    public string? SecretEncrypted { get; set; }

    /// <summary>Comma-separated scope list (e.g. "bills:read,bills:write"). Empty = full tenant R/W.</summary>
    [MaxLength(1000)]
    public string? Scopes { get; set; }

    public DateTime CreatedDate { get; set; }

    [Required, MaxLength(150)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public DateTime? RevokedAt { get; set; }
}
