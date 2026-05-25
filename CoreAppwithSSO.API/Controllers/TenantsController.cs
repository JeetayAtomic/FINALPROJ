using System.Security.Claims;
using System.Text.RegularExpressions;
using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.Models.Identity;
using CoreAppwithSSO.API.Services;
using CoreAppwithSSO.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreAppwithSSO.API.Controllers;

/// <summary>
/// Super-admin-only tenant (client) management.
/// </summary>
[ApiController]
[Route("api/admin/tenants")]
[Authorize(Policy = "SuperAdminOnly")]
public class TenantsController : ControllerBase
{
    private readonly IdentityDbContext _identity;
    private readonly ITenantProvisioner _provisioner;

    public TenantsController(
        IdentityDbContext identity,
        ITenantProvisioner provisioner)
    {
        _identity = identity;
        _provisioner = provisioner;
    }

    [HttpGet]
    public async Task<ActionResult<List<TenantDto>>> List()
    {
        var tenants = await _identity.Tenants
            .OrderBy(t => t.Name)
            .ToListAsync();

        return Ok(tenants.Select(Map).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TenantDto>> Get(int id)
    {
        var tenant = await _identity.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();
        return Ok(Map(tenant));
    }

    [HttpPost]
    public async Task<ActionResult<TenantDto>> Create(CreateTenantDto dto)
    {
        var slug = Slugify(dto.Name);
        if (string.IsNullOrEmpty(slug))
            return BadRequest(new { message = "Name must contain alphanumeric characters." });

        if (await _identity.Tenants.AnyAsync(t => t.Slug == slug))
            return BadRequest(new { message = "A tenant with this name already exists." });

        if (await _identity.Tenants.AnyAsync(t => t.DbName == dto.DbName))
            return BadRequest(new { message = "DbName already in use." });

        if (await _identity.Tenants.AnyAsync(t => t.ClientCode == dto.ClientCode))
            return BadRequest(new { message = "ClientCode already in use." });

        var actor = CurrentUserEmail();

        // Pre-check: if the initial admin email already exists globally, we'll reuse that user
        // (cross-tenant membership). Otherwise we'll create a new one.
        var existingUser = await _identity.Users
            .FirstOrDefaultAsync(u => u.Email == dto.InitialAdminEmail);

        var tenant = new Tenant
        {
            Name = dto.Name,
            Slug = slug,
            DbName = dto.DbName,
            ClientCode = dto.ClientCode,
            TemplateId = dto.TemplateId,
            OrganizationId = dto.OrganizationId,
            ClientFolderPath = dto.ClientFolderPath,
            RawFolderPath = dto.RawFolderPath,
            ProcessedFolderPath = dto.ProcessedFolderPath,
            ErrorFolderPath = dto.ErrorFolderPath,
            PublishFolderPath = dto.PublishFolderPath,
            JsonFolderPath = dto.JsonFolderPath,
            IsActive = true,
            CreatedBy = actor
        };

        _identity.Tenants.Add(tenant);
        await _identity.SaveChangesAsync();

        // Provision DB + seed the initial admin. On any failure, unwind so the admin isn't
        // left with an orphan tenant row or an unusable tenant.
        User initialAdmin;
        try
        {
            await _provisioner.ProvisionAsync(tenant.Id, tenant.DbName);

            initialAdmin = existingUser ?? new User
            {
                FullName = dto.InitialAdminFullName,
                Email = dto.InitialAdminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.InitialAdminPassword),
                IsActive = true
            };
            if (existingUser == null)
            {
                _identity.Users.Add(initialAdmin);
                await _identity.SaveChangesAsync();
            }

            _identity.UserTenants.Add(new UserTenant
            {
                UserId = initialAdmin.Id,
                TenantId = tenant.Id,
                Role = "Admin"
            });
            await _identity.SaveChangesAsync();
        }
        catch
        {
            _identity.Tenants.Remove(tenant);
            // If we just created a new user for this tenant (no prior membership), remove it.
            if (existingUser == null)
            {
                var created = await _identity.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.InitialAdminEmail);
                if (created != null) _identity.Users.Remove(created);
            }
            await _identity.SaveChangesAsync();
            throw;
        }

        return CreatedAtAction(nameof(Get), new { id = tenant.Id }, Map(tenant));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TenantDto>> Update(int id, UpdateTenantDto dto)
    {
        var tenant = await _identity.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();

        // DbName change would require moving data — block it.
        if (!string.Equals(tenant.DbName, dto.DbName, StringComparison.Ordinal))
            return BadRequest(new { message = "DbName cannot be changed after creation." });

        if (!string.Equals(tenant.ClientCode, dto.ClientCode, StringComparison.OrdinalIgnoreCase)
            && await _identity.Tenants.AnyAsync(t => t.Id != id && t.ClientCode == dto.ClientCode))
        {
            return BadRequest(new { message = "ClientCode already in use." });
        }

        tenant.Name = dto.Name;
        tenant.Slug = Slugify(dto.Name);
        tenant.ClientCode = dto.ClientCode;
        tenant.TemplateId = dto.TemplateId;
        tenant.OrganizationId = dto.OrganizationId;
        tenant.ClientFolderPath = dto.ClientFolderPath;
        tenant.RawFolderPath = dto.RawFolderPath;
        tenant.ProcessedFolderPath = dto.ProcessedFolderPath;
        tenant.ErrorFolderPath = dto.ErrorFolderPath;
        tenant.PublishFolderPath = dto.PublishFolderPath;
        tenant.JsonFolderPath = dto.JsonFolderPath;
        tenant.IsActive = dto.IsActive;

        tenant.LastUpdatedDate = DateTime.UtcNow;
        tenant.LastUpdatedBy = CurrentUserEmail();

        await _identity.SaveChangesAsync();

        return Ok(Map(tenant));
    }

    /// <summary>
    /// Seeds a user into the given tenant (typically the first Admin). Reuses an existing
    /// identity user by email if one exists; otherwise creates one with the provided password.
    /// </summary>
    [HttpPost("{id:int}/users")]
    public async Task<ActionResult<TenantUserDto>> SeedTenantUser(
        int id,
        SeedTenantUserDto dto,
        [FromServices] ITenantConnectionResolver resolver)
    {
        var tenant = await _identity.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();
        if (!tenant.IsActive) return BadRequest(new { message = "Tenant is inactive." });

        // Role must exist in the target tenant DB.
        var connStr = resolver.Resolve(tenant.DbName);
        var opts = new DbContextOptionsBuilder<TenantDbContext>().UseSqlServer(connStr).Options;
        await using (var tenantDb = new TenantDbContext(opts))
        {
            var roleExists = await tenantDb.Roles.AnyAsync(r => r.Name == dto.RoleName);
            if (!roleExists)
                return BadRequest(new { message = $"Role '{dto.RoleName}' not found in this tenant." });
        }

        var user = await _identity.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
        {
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                return BadRequest(new { message = "A password of at least 6 characters is required for new users." });

            user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsActive = true
            };
            _identity.Users.Add(user);
            await _identity.SaveChangesAsync();
        }

        var already = await _identity.UserTenants
            .AnyAsync(ut => ut.UserId == user.Id && ut.TenantId == id);
        if (already)
            return BadRequest(new { message = "User is already a member of this tenant." });

        _identity.UserTenants.Add(new UserTenant
        {
            UserId = user.Id,
            TenantId = id,
            Role = dto.RoleName
        });
        await _identity.SaveChangesAsync();

        return Ok(new TenantUserDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = dto.RoleName,
            IsActive = user.IsActive,
            MembershipCreatedAt = DateTime.UtcNow
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var tenant = await _identity.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();

        tenant.IsActive = false;
        tenant.LastUpdatedDate = DateTime.UtcNow;
        tenant.LastUpdatedBy = CurrentUserEmail();
        await _identity.SaveChangesAsync();

        return NoContent();
    }

    private TenantDto Map(Tenant t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Slug = t.Slug,
        DbName = t.DbName,
        ClientCode = t.ClientCode,
        TemplateId = t.TemplateId,
        OrganizationId = t.OrganizationId,
        ClientFolderPath = t.ClientFolderPath,
        RawFolderPath = t.RawFolderPath,
        ProcessedFolderPath = t.ProcessedFolderPath,
        ErrorFolderPath = t.ErrorFolderPath,
        PublishFolderPath = t.PublishFolderPath,
        JsonFolderPath = t.JsonFolderPath,
        IsActive = t.IsActive,
        CreatedDate = t.CreatedDate,
        CreatedBy = t.CreatedBy,
        LastUpdatedDate = t.LastUpdatedDate,
        LastUpdatedBy = t.LastUpdatedBy
    };

    private string CurrentUserEmail()
        => User.FindFirstValue(ClaimTypes.Email) ?? "unknown";

    private static string Slugify(string input)
    {
        var lower = (input ?? "").Trim().ToLowerInvariant();
        return Regex.Replace(lower, @"[^a-z0-9]+", "_").Trim('_');
    }
}
