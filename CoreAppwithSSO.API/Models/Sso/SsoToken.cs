using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.Models.Sso;

/// <summary>
/// One-time SSO redirect token. UserId/TenantId reference rows in the Identity DB
/// (no cross-DB FK). ApplicationId references a row in the same SSO DB.
/// </summary>
public class SsoToken
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Token { get; set; } = string.Empty;

    public int UserId { get; set; }

    public int TenantId { get; set; }

    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }
}
