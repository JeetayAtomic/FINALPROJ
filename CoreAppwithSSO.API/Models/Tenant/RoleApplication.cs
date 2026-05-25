namespace CoreAppwithSSO.API.Models.Tenant;

/// <summary>
/// App access granted to a role. ApplicationId references the SSO DB catalog (no cross-DB FK).
/// </summary>
public class RoleApplication
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public int ApplicationId { get; set; }
}
