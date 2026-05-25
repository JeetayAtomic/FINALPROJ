using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.Models.Sso;

/// <summary>
/// Global application catalog entry. Tenants subscribe via <see cref="TenantApplication"/>.
/// </summary>
public class Application
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    [MaxLength(100)]
    public string IconName { get; set; } = "apps";

    [MaxLength(20)]
    public string IconColor { get; set; } = "#4285F4";

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<TenantApplication> TenantApplications { get; set; } = new List<TenantApplication>();
}
