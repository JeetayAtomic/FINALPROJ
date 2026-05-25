using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.DTOs;

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class TenantSummaryDto
{
    public int TenantId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Response from POST /auth/login.
/// - Super admin: <see cref="Token"/> is populated (tenant-less JWT) and <see cref="IsSuperAdmin"/> is true.
/// - Regular user with 1 tenant: server could still return just the tenant list; client calls select-tenant.
/// - Regular user with many tenants: interim token + tenants list; client calls select-tenant.
/// </summary>
public class LoginResponseDto
{
    public bool IsSuperAdmin { get; set; }

    /// <summary>Final JWT (super admin only). Empty for regular users until select-tenant.</summary>
    public string Token { get; set; } = string.Empty;

    public string InterimToken { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<TenantSummaryDto> Tenants { get; set; } = new();
}

public class SelectTenantDto
{
    [Required]
    public int TenantId { get; set; }
}

/// <summary>Tenant-scoped JWT response (after select-tenant).</summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Server-side session id. The dashboard polls /api/sso/sessions/{id} for
    /// revocation (cross-app logout sync) and calls /api/sso/logout with this id.
    /// </summary>
    public int SessionId { get; set; }
}

public class ApplicationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class SSORedirectDto
{
    public string RedirectUrl { get; set; } = string.Empty;
    public string SsoToken { get; set; } = string.Empty;
}

public class SSOValidationDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class SSOValidationResponseDto
{
    public bool IsValid { get; set; }
    /// <summary>Server-side session id created on successful validation. Used for polling + logout.</summary>
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public int TenantId { get; set; }
    public int ApplicationId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class SessionStatusDto
{
    public bool Active { get; set; }
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public int TenantId { get; set; }
    public int ApplicationId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class LogoutDto
{
    public int SessionId { get; set; }
}
