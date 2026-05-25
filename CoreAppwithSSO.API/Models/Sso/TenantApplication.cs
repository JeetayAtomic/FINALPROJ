using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.Models.Sso;

/// <summary>
/// Tenant subscription to a global <see cref="Application"/>.
/// TenantId references a Tenant row in the Identity DB (no cross-DB FK).
/// </summary>
public class TenantApplication
{
    public int TenantId { get; set; }

    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime AssignedAt { get; set; }
}
