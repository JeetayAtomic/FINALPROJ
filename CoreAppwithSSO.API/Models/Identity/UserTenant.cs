using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.Models.Identity;

/// <summary>
/// Membership of a user in a tenant, with the user's per-tenant role.
/// </summary>
public class UserTenant
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Role { get; set; } = "User";

    public DateTime CreatedAt { get; set; }
}
