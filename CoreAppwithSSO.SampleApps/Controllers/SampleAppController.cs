using CoreAppwithSSO.SampleApps.Services;
using CoreAppwithSSO.SampleApps.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace CoreAppwithSSO.SampleApps.Controllers;

/// <summary>
/// Base controller for the three sample apps. Each subclass supplies a display name,
/// a URL slug, an accent color, and its dashboard tiles; the SSO/cookie/logout plumbing is shared.
/// Each app runs on its own URL (different origin), so Sign-In / Logout / revoked-session
/// all funnel the browser to the central SSO Login page (configured via Dashboard:LoginUrl).
/// </summary>
public abstract class SampleAppController : ControllerBase
{
    public const string SessionCookieName = "sso_session_id";
    private const string DefaultLoginUrl = "http://localhost:4200/login";

    private readonly ISsoValidator _sso;
    private readonly IConfiguration _config;

    protected SampleAppController(ISsoValidator sso, IConfiguration config)
    {
        _sso = sso;
        _config = config;
    }

    protected abstract string AppName { get; }
    protected abstract string AppSlug { get; }
    protected abstract string AccentColor { get; }
    protected abstract IReadOnlyList<DashboardTile> Tiles { get; }

    /// <summary>
    /// Central SSO login URL with this app's root attached as <c>returnUrl</c>, so that
    /// after re-authenticating, the dashboard can mint an SSO token and bounce the user
    /// back to the app they were on.
    /// </summary>
    private string LoginUrl
    {
        get
        {
            var baseLogin = _config["Dashboard:LoginUrl"] ?? DefaultLoginUrl;
            var returnUrl = $"{Request.Scheme}://{Request.Host}/{AppSlug}";
            return QueryHelpers.AddQueryString(baseLogin, "returnUrl", returnUrl);
        }
    }

    protected async Task<IActionResult> ShowIndexAsync(CancellationToken ct)
    {
        if (TryGetSessionId(out var sessionId))
        {
            var status = await _sso.GetSessionAsync(sessionId, ct);
            if (status.Active)
                return AppPageRenderer.Dashboard(AppName, AppSlug, AccentColor, Tiles, status, LoginUrl);

            // Session was revoked (likely logout from another app) — clear the stale cookie.
            ClearSessionCookie();
        }
        return AppPageRenderer.Landing(AppName, AppSlug, AccentColor, LoginUrl);
    }

    protected async Task<IActionResult> HandleSsoCallbackAsync(string? token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return AppPageRenderer.Denied(AppName, AppSlug, AccentColor, "Missing token in query string.", LoginUrl);

        var result = await _sso.ValidateAsync(token, ct);
        if (!result.IsValid)
            return AppPageRenderer.Denied(AppName, AppSlug, AccentColor,
                "Token was rejected by the Dashboard (expired, already used, or invalid).", LoginUrl);

        SetSessionCookie(result.SessionId);
        // Land on the slug root so a refresh doesn't re-hit the (now-consumed) callback URL.
        return Redirect($"/{AppSlug}");
    }

    protected async Task<IActionResult> HandleLogoutAsync(CancellationToken ct)
    {
        if (TryGetSessionId(out var sessionId))
        {
            await _sso.LogoutAsync(sessionId, ct);
        }
        ClearSessionCookie();
        // Per SSO contract: after sign-out, send the user to the central Login page.
        return Redirect(LoginUrl);
    }

    protected async Task<IActionResult> HandleSessionStatusAsync(CancellationToken ct)
    {
        if (!TryGetSessionId(out var sessionId))
            return Ok(new { active = false });

        var status = await _sso.GetSessionAsync(sessionId, ct);
        if (!status.Active)
        {
            ClearSessionCookie();
            return Ok(new { active = false });
        }
        return Ok(new
        {
            active = true,
            sessionId = status.SessionId,
            userId = status.UserId,
            tenantId = status.TenantId,
            email = status.Email,
            fullName = status.FullName,
            role = status.Role
        });
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
            // Lax lets the cookie survive the cross-site redirect from the Dashboard back to /sso/callback.
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

public record DashboardTile(string Title, string Subtitle, string Metric, string Icon);

[ApiController]
[Route("hr")]
[Host("*:5301")]
public class HrController : SampleAppController
{
    public HrController(ISsoValidator sso, IConfiguration config) : base(sso, config) { }

    protected override string AppName => "HR Portal";
    protected override string AppSlug => "hr";
    protected override string AccentColor => "#4285F4";
    protected override IReadOnlyList<DashboardTile> Tiles => new[]
    {
        new DashboardTile("Employees",        "Active headcount across departments",  "248",  "👥"),
        new DashboardTile("Open Positions",   "Recruitment pipeline",                  "12",   "📋"),
        new DashboardTile("Leave Requests",   "Awaiting your approval",                "7",    "🗓️"),
        new DashboardTile("Payroll",          "Next run · 28th",                       "₹38L", "💰"),
        new DashboardTile("Performance",      "Reviews due this cycle",                "34",   "⭐"),
        new DashboardTile("Training",         "Active enrollments",                    "19",   "🎓"),
    };

    [HttpGet("")] public Task<IActionResult> Index(CancellationToken ct) => ShowIndexAsync(ct);
    [HttpGet("sso/callback")] public Task<IActionResult> Callback([FromQuery] string? token, CancellationToken ct)
        => HandleSsoCallbackAsync(token, ct);
    [HttpPost("logout")] public Task<IActionResult> Logout(CancellationToken ct) => HandleLogoutAsync(ct);
    [HttpGet("session-status")] public Task<IActionResult> SessionStatus(CancellationToken ct) => HandleSessionStatusAsync(ct);
}

[ApiController]
[Route("finance")]
[Host("*:5302")]
public class FinanceController : SampleAppController
{
    public FinanceController(ISsoValidator sso, IConfiguration config) : base(sso, config) { }

    protected override string AppName => "Finance";
    protected override string AppSlug => "finance";
    protected override string AccentColor => "#EA4335";
    protected override IReadOnlyList<DashboardTile> Tiles => new[]
    {
        new DashboardTile("Open Invoices",    "Awaiting payment",               "42",     "🧾"),
        new DashboardTile("Receivables",      "Outstanding this quarter",       "₹12.4Cr","📈"),
        new DashboardTile("Payables",         "Due in next 30 days",            "₹4.1Cr", "📉"),
        new DashboardTile("Expense Claims",   "Pending approval",               "18",     "💳"),
        new DashboardTile("Budgets",          "Departments tracking on target", "9/12",   "🎯"),
        new DashboardTile("Audit Tasks",      "Items in review",                "5",      "🔍"),
    };

    [HttpGet("")] public Task<IActionResult> Index(CancellationToken ct) => ShowIndexAsync(ct);
    [HttpGet("sso/callback")] public Task<IActionResult> Callback([FromQuery] string? token, CancellationToken ct)
        => HandleSsoCallbackAsync(token, ct);
    [HttpPost("logout")] public Task<IActionResult> Logout(CancellationToken ct) => HandleLogoutAsync(ct);
    [HttpGet("session-status")] public Task<IActionResult> SessionStatus(CancellationToken ct) => HandleSessionStatusAsync(ct);
}

[ApiController]
[Route("inventory")]
[Host("*:5303")]
public class InventoryController : SampleAppController
{
    public InventoryController(ISsoValidator sso, IConfiguration config) : base(sso, config) { }

    protected override string AppName => "Inventory";
    protected override string AppSlug => "inventory";
    protected override string AccentColor => "#FF6D01";
    protected override IReadOnlyList<DashboardTile> Tiles => new[]
    {
        new DashboardTile("SKUs in Catalog",   "Across all categories",         "1,284", "📦"),
        new DashboardTile("Low Stock Alerts",  "Below reorder threshold",        "23",    "⚠️"),
        new DashboardTile("Purchase Orders",   "Awaiting receipt",               "16",    "🛒"),
        new DashboardTile("Suppliers",         "Active vendor list",             "57",    "🏭"),
        new DashboardTile("Warehouses",        "Locations online",               "4",     "🏬"),
        new DashboardTile("Shipments Today",   "In transit",                     "31",    "🚚"),
    };

    [HttpGet("")] public Task<IActionResult> Index(CancellationToken ct) => ShowIndexAsync(ct);
    [HttpGet("sso/callback")] public Task<IActionResult> Callback([FromQuery] string? token, CancellationToken ct)
        => HandleSsoCallbackAsync(token, ct);
    [HttpPost("logout")] public Task<IActionResult> Logout(CancellationToken ct) => HandleLogoutAsync(ct);
    [HttpGet("session-status")] public Task<IActionResult> SessionStatus(CancellationToken ct) => HandleSessionStatusAsync(ct);
}
