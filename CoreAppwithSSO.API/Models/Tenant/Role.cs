using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.Models.Tenant;

/// <summary>
/// Per-tenant role definition. System roles (Admin, User) are seeded by the provisioner
/// and cannot be deleted. IdentityDb.UserTenants.Role holds this role's Name.
/// </summary>
public class Role
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }

    /// <summary>Seeded system role (cannot be deleted or renamed).</summary>
    public bool IsSystem { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<RoleApplication> RoleApplications { get; set; } = new List<RoleApplication>();
}
