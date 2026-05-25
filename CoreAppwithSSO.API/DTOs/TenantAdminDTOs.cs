using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.DTOs;

/// <summary>Tenant representation for the super-admin UI.</summary>
public class TenantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string DbName { get; set; } = string.Empty;
    public string ClientCode { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public string? OrganizationId { get; set; }

    public string? ClientFolderPath { get; set; }
    public string? RawFolderPath { get; set; }
    public string? ProcessedFolderPath { get; set; }
    public string? ErrorFolderPath { get; set; }
    public string? PublishFolderPath { get; set; }
    public string? JsonFolderPath { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastUpdatedDate { get; set; }
    public string? LastUpdatedBy { get; set; }
}

/// <summary>Shared tenant fields used by both create and update payloads.</summary>
public abstract class TenantWriteDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Physical DB name chosen by super admin (must be unique).</summary>
    [Required, MaxLength(128), RegularExpression(@"^[A-Za-z0-9_]+$",
        ErrorMessage = "DbName may contain only letters, digits, and underscores.")]
    public string DbName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string ClientCode { get; set; } = string.Empty;

    [MaxLength(50)] public string? TemplateId { get; set; }
    [MaxLength(50)] public string? OrganizationId { get; set; }

    [MaxLength(500)] public string? ClientFolderPath { get; set; }
    [MaxLength(500)] public string? RawFolderPath { get; set; }
    [MaxLength(500)] public string? ProcessedFolderPath { get; set; }
    [MaxLength(500)] public string? ErrorFolderPath { get; set; }
    [MaxLength(500)] public string? PublishFolderPath { get; set; }
    [MaxLength(500)] public string? JsonFolderPath { get; set; }
}

/// <summary>
/// Create a tenant AND its initial administrator in one atomic call.
/// </summary>
public class CreateTenantDto : TenantWriteDto
{
    [Required, MaxLength(100)]
    public string InitialAdminFullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string InitialAdminEmail { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string InitialAdminPassword { get; set; } = string.Empty;
}

public class UpdateTenantDto : TenantWriteDto
{
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Super-admin payload to seed an additional user into an existing tenant (e.g. replacing a locked-out admin).
/// </summary>
public class SeedTenantUserDto
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Required only if the email is new to the platform.</summary>
    [MinLength(6)]
    public string? Password { get; set; }

    [Required, MaxLength(50)]
    public string RoleName { get; set; } = "Admin";
}
