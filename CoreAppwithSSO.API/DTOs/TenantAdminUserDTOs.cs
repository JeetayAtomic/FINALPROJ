using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.DTOs;

public class TenantUserDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime MembershipCreatedAt { get; set; }
}

public class CreateTenantUserDto
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Required only when the email is new to the platform; ignored if an existing identity
    /// user is being invited into this tenant.
    /// </summary>
    [MinLength(6)]
    public string? Password { get; set; }

    [Required, MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;
}

public class UpdateTenantUserDto
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class ResetPasswordDto
{
    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<int> ApplicationIds { get; set; } = new();
    public int MemberCount { get; set; }
}

public class CreateRoleDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }

    public List<int> ApplicationIds { get; set; } = new();
}

public class UpdateRoleDto : CreateRoleDto { }
