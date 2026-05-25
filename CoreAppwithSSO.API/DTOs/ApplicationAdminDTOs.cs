using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.API.DTOs;

/// <summary>Full Application row exposed to super-admin (includes IsActive + audit).</summary>
public class ApplicationAdminDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public abstract class ApplicationWriteDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(500), Url]
    public string BaseUrl { get; set; } = string.Empty;

    [MaxLength(100)]
    public string IconName { get; set; } = "apps";

    [MaxLength(20)]
    public string IconColor { get; set; } = "#4285F4";

    public int DisplayOrder { get; set; }
}

public class CreateApplicationDto : ApplicationWriteDto { }

public class UpdateApplicationDto : ApplicationWriteDto
{
    public bool IsActive { get; set; } = true;
}

/// <summary>Subscription view: every catalog app paired with this tenant's subscription state.</summary>
public class TenantApplicationDto
{
    public int ApplicationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool ApplicationActive { get; set; }

    /// <summary>True if the tenant has a TenantApplications row for this app.</summary>
    public bool IsSubscribed { get; set; }

    /// <summary>Subscription's IsActive flag (the tenant-side toggle).</summary>
    public bool SubscriptionActive { get; set; }

    public DateTime? AssignedAt { get; set; }
}

public class TenantApplicationWriteDto
{
    public bool IsActive { get; set; } = true;
}
