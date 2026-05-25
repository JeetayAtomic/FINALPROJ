using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.Models.Sso;

/// <summary>
/// A server-side session created when a target app successfully validates an SSO token.
/// Target apps poll the session's active state and can trigger platform-wide logout by
/// revoking every session for a (UserId, TenantId) pair.
/// </summary>
public class UserSession
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TenantId { get; set; }

    /// <summary>
    /// 0 indicates a dashboard (non-SSO) session; otherwise the target Application's id.
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>Null for dashboard sessions (no SSO redirect involved).</summary>
    public int? SsoTokenId { get; set; }
    public SsoToken? SsoToken { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsActive => RevokedAt == null;
}
