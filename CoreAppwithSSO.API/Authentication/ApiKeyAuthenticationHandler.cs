using System.Security.Claims;
using System.Text.Encodings.Web;
using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.Services;
using CoreAppwithSSO.API.Tenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoreAppwithSSO.API.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
    public const string HeaderName = "X-Api-Key";
}

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public const string ApiKeyIdClaim = "api_key_id";
    public const string ApiKeyScopeClaim = "scope";
    public const string ApiKeyRole = "ApiKey";

    private readonly IdentityDbContext _identity;
    private readonly IApiKeyService _keys;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IdentityDbContext identity,
        IApiKeyService keys)
        : base(options, logger, encoder)
    {
        _identity = identity;
        _keys = keys;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var values))
            return AuthenticateResult.NoResult();

        var presented = values.ToString();
        if (string.IsNullOrWhiteSpace(presented))
            return AuthenticateResult.NoResult();

        var prefix = _keys.ExtractPrefix(presented);
        if (prefix == null)
            return AuthenticateResult.Fail("Malformed API key.");

        var record = await _identity.ApiKeys
            .Include(k => k.Tenant)
            .FirstOrDefaultAsync(k => k.KeyPrefix == prefix);

        if (record == null || record.Tenant == null || !record.Tenant.IsActive)
            return AuthenticateResult.Fail("Invalid API key.");

        if (!_keys.Verify(presented, record.KeyHash))
            return AuthenticateResult.Fail("Invalid API key.");

        if (record.RevokedAt.HasValue)
            return AuthenticateResult.Fail("API key has been revoked.");

        if (record.ExpiresAt.HasValue && record.ExpiresAt.Value < DateTime.UtcNow)
            return AuthenticateResult.Fail("API key has expired.");

        // Update last-used timestamp (best-effort; don't block the request if it fails).
        try
        {
            record.LastUsedAt = DateTime.UtcNow;
            await _identity.SaveChangesAsync();
        }
        catch
        {
            // swallow — never block a valid request on telemetry writes
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, $"apikey:{record.Id}"),
            new(ClaimTypes.Name, record.Name),
            new(ClaimTypes.Role, ApiKeyRole),
            new(TenantContextMiddleware.TenantIdClaim, record.TenantId.ToString()),
            new(JwtService.TokenUseClaim, JwtService.TokenUseTenant),
            new(ApiKeyIdClaim, record.Id.ToString())
        };

        if (!string.IsNullOrWhiteSpace(record.Scopes))
        {
            foreach (var s in record.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                claims.Add(new Claim(ApiKeyScopeClaim, s));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
