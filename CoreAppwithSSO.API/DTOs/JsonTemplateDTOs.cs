using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.DTOs;

/// <summary>Summary shown in the list (no JSON body — keeps the list response lightweight).</summary>
public class JsonTemplateSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastUpdatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }
}

/// <summary>Full template including JSON content (returned by GET /{id}).</summary>
public class JsonTemplateDto : JsonTemplateSummaryDto
{
    public string JsonContent { get; set; } = string.Empty;
}

public class CreateJsonTemplateDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>Must be valid JSON.</summary>
    [Required]
    public string JsonContent { get; set; } = "{}";
}

public class UpdateJsonTemplateDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>Must be valid JSON.</summary>
    [Required]
    public string JsonContent { get; set; } = "{}";
}
