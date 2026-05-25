using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.Models.Identity;

/// <summary>
/// Singleton key-value store for cross-tenant configuration that needs to be rotatable
/// at runtime without redeployment (e.g. the API-key lookup master secret hash).
/// </summary>
public class AppSetting
{
    [Key, MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }

    [MaxLength(150)]
    public string? UpdatedBy { get; set; }
}
