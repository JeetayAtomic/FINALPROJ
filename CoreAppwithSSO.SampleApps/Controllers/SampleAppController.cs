using CoreAppwithSSO.SampleApps.Services;
using CoreAppwithSSO.SampleApps.Views;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.SampleApps.Controllers;

/// <summary>
/// Base controller for the three sample apps. Each subclass supplies a display name,
/// a URL slug, and an accent color; behavior (landing + SSO callback) is shared.
/// </summary>
public abstract class SampleAppController : ControllerBase
{
    private readonly ISsoValidator _sso;

    protected SampleAppController(ISsoValidator sso) { _sso = sso; }

    protected abstract string AppName { get; }
    protected abstract string AppSlug { get; }
    protected abstract string AccentColor { get; }

    protected IActionResult LandingPage() =>
        AppPageRenderer.Landing(AppName, AppSlug, AccentColor);

    protected async Task<IActionResult> HandleSsoCallbackAsync(string? token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return AppPageRenderer.Denied(AppName, AppSlug, AccentColor, "Missing token in query string.");

        var result = await _sso.ValidateAsync(token, ct);
        return result.IsValid
            ? AppPageRenderer.Welcome(AppName, AppSlug, AccentColor, result)
            : AppPageRenderer.Denied(AppName, AppSlug, AccentColor,
                "Token was rejected by the Dashboard (expired, already used, or invalid).");
    }
}

[ApiController]
[Route("hr")]
public class HrController : SampleAppController
{
    public HrController(ISsoValidator sso) : base(sso) { }

    protected override string AppName => "HR Portal";
    protected override string AppSlug => "hr";
    protected override string AccentColor => "#4285F4";

    [HttpGet("")] public IActionResult Index() => LandingPage();
    [HttpGet("sso/callback")] public Task<IActionResult> Callback([FromQuery] string? token, CancellationToken ct)
        => HandleSsoCallbackAsync(token, ct);
}

[ApiController]
[Route("finance")]
public class FinanceController : SampleAppController
{
    public FinanceController(ISsoValidator sso) : base(sso) { }

    protected override string AppName => "Finance";
    protected override string AppSlug => "finance";
    protected override string AccentColor => "#EA4335";

    [HttpGet("")] public IActionResult Index() => LandingPage();
    [HttpGet("sso/callback")] public Task<IActionResult> Callback([FromQuery] string? token, CancellationToken ct)
        => HandleSsoCallbackAsync(token, ct);
}

[ApiController]
[Route("inventory")]
public class InventoryController : SampleAppController
{
    public InventoryController(ISsoValidator sso) : base(sso) { }

    protected override string AppName => "Inventory";
    protected override string AppSlug => "inventory";
    protected override string AccentColor => "#FF6D01";

    [HttpGet("")] public IActionResult Index() => LandingPage();
    [HttpGet("sso/callback")] public Task<IActionResult> Callback([FromQuery] string? token, CancellationToken ct)
        => HandleSsoCallbackAsync(token, ct);
}
