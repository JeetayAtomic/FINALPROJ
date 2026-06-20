using System.Net.Http.Json;

namespace CoreAppwithSSO.ElectionTracker.Services;

public record SsoValidationResult(
    bool IsValid,
    int SessionId,
    int UserId,
    int TenantId,
    int ApplicationId,
    string Email,
    string FullName,
    string Role);

public record SsoSessionStatus(
    bool Active,
    int SessionId,
    int UserId,
    int TenantId,
    int ApplicationId,
    string Email,
    string FullName,
    string Role);

public interface ISsoValidator
{
    Task<SsoValidationResult> ValidateAsync(string token, CancellationToken ct = default);
    Task<SsoSessionStatus> GetSessionAsync(int sessionId, CancellationToken ct = default);
    Task LogoutAsync(int sessionId, CancellationToken ct = default);
}

/// <summary>
/// Server-side bridge to the central SSO API. Validates one-time tokens, polls
/// session status (so the app can detect logout from another app), and triggers global logout.
/// All trust decisions are delegated to the SSO API — this app never mints or trusts
/// anything client-supplied beyond an opaque session id it re-validates on every request.
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
                return InvalidValidation();

            var body = await response.Content.ReadFromJsonAsync<SsoValidationApiResponse>(cancellationToken: ct);
            if (body == null || !body.IsValid) return InvalidValidation();

            return new SsoValidationResult(
                true,
                body.SessionId,
                body.UserId,
                body.TenantId,
                body.ApplicationId,
                body.Email ?? "",
                body.FullName ?? "",
                body.Role ?? "");
        }
        catch
        {
            return InvalidValidation();
        }
    }

    public async Task<SsoSessionStatus> GetSessionAsync(int sessionId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"api/sso/sessions/{sessionId}", ct);
            if (!response.IsSuccessStatusCode) return InactiveSession(sessionId);

            var body = await response.Content.ReadFromJsonAsync<SessionStatusApiResponse>(cancellationToken: ct);
            if (body == null) return InactiveSession(sessionId);

            return new SsoSessionStatus(
                body.Active,
                body.SessionId,
                body.UserId,
                body.TenantId,
                body.ApplicationId,
                body.Email ?? "",
                body.FullName ?? "",
                body.Role ?? "");
        }
        catch
        {
            return InactiveSession(sessionId);
        }
    }

    public async Task LogoutAsync(int sessionId, CancellationToken ct = default)
    {
        try
        {
            await _http.PostAsJsonAsync("api/sso/logout", new { SessionId = sessionId }, ct);
        }
        catch
        {
            // Swallow — local cookie clear still happens, and the SSO API treats logout as idempotent.
        }
    }

    private static SsoValidationResult InvalidValidation() => new(false, 0, 0, 0, 0, "", "", "");
    private static SsoSessionStatus InactiveSession(int sessionId) => new(false, sessionId, 0, 0, 0, "", "", "");

    private sealed class SsoValidationApiResponse
    {
        public bool IsValid { get; set; }
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public int ApplicationId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }

    private sealed class SessionStatusApiResponse
    {
        public bool Active { get; set; }
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public int ApplicationId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
    }
}
