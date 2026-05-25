using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.Models.Identity;

public class Tenant
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-friendly identifier derived from Name (e.g., "acme").</summary>
    [Required, MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>Physical per-tenant database name (super admin decides the value).</summary>
    [Required, MaxLength(128)]
    public string DbName { get; set; } = string.Empty;

    // ---------- Business identifiers ----------
    [Required, MaxLength(50)]
    public string ClientCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TemplateId { get; set; }

    [MaxLength(50)]
    public string? OrganizationId { get; set; }

    // ---------- Folder paths ----------
    [MaxLength(500)]
    public string? ClientFolderPath { get; set; }

    [MaxLength(500)]
    public string? RawFolderPath { get; set; }

    [MaxLength(500)]
    public string? ProcessedFolderPath { get; set; }

    [MaxLength(500)]
    public string? ErrorFolderPath { get; set; }

    [MaxLength(500)]
    public string? PublishFolderPath { get; set; }

    [MaxLength(500)]
    public string? JsonFolderPath { get; set; }

    // ---------- State + audit ----------
    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; }

    [MaxLength(150)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? LastUpdatedDate { get; set; }

    [MaxLength(150)]
    public string? LastUpdatedBy { get; set; }

    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
}
