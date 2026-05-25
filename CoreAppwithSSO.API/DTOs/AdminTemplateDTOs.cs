namespace CoreAppwithSSO.API.DTOs;

/// <summary>One row in the cross-tenant template grid (super admin view).</summary>
public class AdminTemplateRowDto
{
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string ClientCode { get; set; } = string.Empty;
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastUpdatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }
}

/// <summary>Lookup response: tenant details + the matched template's full content.</summary>
public class AdminLookupResponseDto
{
    public TenantDto Tenant { get; set; } = new();
    public JsonTemplateDto Template { get; set; } = new();
}
