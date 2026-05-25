using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.Models.Tenant;

public class JsonTemplate
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>Freeform JSON content. Validated as parseable JSON on write.</summary>
    [Required]
    public string JsonContent { get; set; } = "{}";

    public int Version { get; set; } = 1;

    public DateTime CreatedAt { get; set; }

    [Required, MaxLength(150)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? LastUpdatedAt { get; set; }

    [MaxLength(150)]
    public string? LastUpdatedBy { get; set; }
}
