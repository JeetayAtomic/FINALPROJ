using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.Models.Identity;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Cross-tenant platform administrator. Super admins do NOT have rows in
    /// <see cref="UserTenant"/> — they are not members of any tenant.
    /// </summary>
    public bool IsSuperAdmin { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
}
