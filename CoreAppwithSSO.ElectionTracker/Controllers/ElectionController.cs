using CoreAppwithSSO.ElectionTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.ElectionTracker.Controllers;

/// <summary>
/// JSON API for the Election Tracker Angular front-end. There is no local login:
/// access is established only by a one-time SSO token minted by the central dashboard
/// for THIS application. The backend validates the token against the central SSO API and
/// holds the session in an HttpOnly cookie — the SPA never sees anything it could forge.
/// Every data endpoint re-checks the session, so a missing/expired/wrong-app token yields 401.
/// </summary>
[ApiController]
[Route("api/election")]
public class ElectionController : ControllerBase
{
    public const string SessionCookieName = "et_session_id";
    private const string DefaultLoginUrl = "http://localhost:4200/login";

    private readonly ISsoValidator _sso;
    private readonly IConfiguration _config;

    public ElectionController(ISsoValidator sso, IConfiguration config)
    {
        _sso = sso;
        _config = config;
    }

    /// <summary>The Application.Id this app was registered as in the SSO catalog (ELECTION).</summary>
    private int ExpectedApplicationId => _config.GetValue<int>("Sso:ApplicationId");

    private string LoginUrl => _config["Dashboard:LoginUrl"] ?? DefaultLoginUrl;

    public record SessionInfoDto(int UserId, int TenantId, string Email, string FullName, string Role);
    public record ElectionTileDto(string Title, string Subtitle, string Metric, string Icon);
    public record ElectionDataDto(string GeneratedAtUtc, IReadOnlyList<ElectionTileDto> Tiles);

    /// <summary>
    /// Exchange a one-time SSO token (from the dashboard redirect) for a server-side session.
    /// On success, sets the HttpOnly session cookie and returns the signed-in user.
    /// </summary>
    [HttpPost("sso/callback")]
    public async Task<IActionResult> Callback([FromBody] CallbackRequest body, CancellationToken ct)
    {
        if (body == null || string.IsNullOrWhiteSpace(body.Token))
            return Unauthorized(new { message = "No SSO token supplied." });

        var result = await _sso.ValidateAsync(body.Token, ct);
        if (!result.IsValid)
            return Unauthorized(new { message = "The SSO token was rejected (expired, already used, or invalid)." });

        // Defence in depth: a token minted for a different application must not unlock this one.
        if (result.ApplicationId != ExpectedApplicationId)
            return Unauthorized(new { message = "This SSO token was issued for a different application." });

        SetSessionCookie(result.SessionId);
        return Ok(new SessionInfoDto(result.UserId, result.TenantId, result.Email, result.FullName, result.Role));
    }

    /// <summary>Current signed-in user, re-validated against the SSO API. 401 if no active session for this app.</summary>
    [HttpGet("session")]
    public async Task<IActionResult> Session(CancellationToken ct)
    {
        var status = await RequireActiveSessionAsync(ct);
        if (status == null) return Unauthorized(new { message = "Not signed in." });

        return Ok(new SessionInfoDto(status.UserId, status.TenantId, status.Email, status.FullName, status.Role));
    }

    /// <summary>Election dashboard data — only served to an active, validated session.</summary>
    [HttpGet("data")]
    public async Task<IActionResult> Data(CancellationToken ct)
    {
        var status = await RequireActiveSessionAsync(ct);
        if (status == null) return Unauthorized(new { message = "Not signed in." });

        var tiles = new List<ElectionTileDto>
        {
            new("Registered Voters",   "Across all precincts",   "1,284,502", "🗳️"),
            new("Precincts Reporting", "Live count",             "342 / 512", "📊"),
            new("Ballots Counted",     "Updated 2 min ago",      "68.4%",     "✅"),
            new("Leading Candidate",   "Statewide tally",        "A. Sharma", "🏆"),
            new("Voter Turnout",       "vs. last election",      "+4.2%",     "📈"),
            new("Pending Ballots",     "Mail-in + provisional",  "12,904",    "📨"),
        };

        return Ok(new ElectionDataDto(DateTime.UtcNow.ToString("u"), tiles));
    }

    /// <summary>Global sign-out: revokes the session everywhere and clears the local cookie.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (TryGetSessionId(out var sessionId))
            await _sso.LogoutAsync(sessionId, ct);

        ClearSessionCookie();
        return Ok(new { loginUrl = LoginUrl });
    }

    public record CallbackRequest(string Token);

    private async Task<SsoSessionStatus?> RequireActiveSessionAsync(CancellationToken ct)
    {
        if (!TryGetSessionId(out var sessionId))
            return null;

        var status = await _sso.GetSessionAsync(sessionId, ct);
        if (!status.Active || status.ApplicationId != ExpectedApplicationId)
        {
            ClearSessionCookie();
            return null;
        }
        return status;
    }

    private bool TryGetSessionId(out int sessionId)
    {
        sessionId = 0;
        return Request.Cookies.TryGetValue(SessionCookieName, out var raw)
               && int.TryParse(raw, out sessionId)
               && sessionId > 0;
    }

    private void SetSessionCookie(int sessionId)
    {
        Response.Cookies.Append(SessionCookieName, sessionId.ToString(), new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });
    }

    private void ClearSessionCookie()
    {
        Response.Cookies.Delete(SessionCookieName, new CookieOptions { Path = "/" });
    }
}
