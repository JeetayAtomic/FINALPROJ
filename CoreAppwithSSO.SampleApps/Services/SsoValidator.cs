using System.Net.Http.Json;

namespace CoreAppwithSSO.SampleApps.Services;

public record SsoValidationResult(
    bool IsValid,
    int UserId,
    int TenantId,
    string Email,
    string FullName,
    string Role);

public interface ISsoValidator
{
    Task<SsoValidationResult> ValidateAsync(string token, CancellationToken ct = default);
}

/// <summary>
/// Server-side validator that posts a one-time SSO token to the Dashboard API and returns
/// the authenticated user's identity. Runs on the sample app's backend — no CORS concerns.
/// </summary>
public class SsoValidator : ISsoValidator
{
    private readonly HttpClient _http;

    public SsoValidator(HttpClient http)
    {
        _http = http;
    }

    public async Task<SsoValidationResult> ValidateAsync(string token, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/sso/validate", new { Token = token }, ct);
            if (!response.IsSuccessStatusCode)
                return Invalid();

            var body = await response.Content.ReadFromJsonAsync<SsoValidationApiResponse>(cancellationToken: ct);
            if (body == null || !body.IsValid) return Invalid();

            return new SsoValidationResult(
                true,
                body.UserId,
                body.TenantId,
                body.Email ?? "",
                body.FullName ?? "",
                body.Role ?? "");
        }
        catch
        {
            return Invalid();
        }
    }

    private static SsoValidationResult Invalid() => new(false, 0, 0, "", "", "");

    private sealed class SsoValidationApiResponse
    {
        public bool IsValid { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }
}
