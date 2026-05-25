using System.Net;
using System.Text;
using CoreAppwithSSO.SampleApps.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.SampleApps.Views;

/// <summary>Simple HTML page renderer. Keeps the sample apps lightweight — no Razor, no SPA.</summary>
public static class AppPageRenderer
{
    public static IActionResult Landing(string appName, string appSlug, string accent)
    {
        var html = BuildPage(appName, appSlug, accent, $$"""
            <section class="card">
              <h1>{{HtmlEncode(appName)}}</h1>
              <p class="muted">This is a sample target application for testing SSO.</p>
              <p>You reached this page directly (no SSO token). The Dashboard redirects here after SSO validation.</p>
              <p class="muted">Expected callback URL: <code>/{{HtmlEncode(appSlug)}}/sso/callback?token=&lt;one-time-token&gt;</code></p>
            </section>
            """);
        return new ContentResult { Content = html, ContentType = "text/html", StatusCode = 200 };
    }

    public static IActionResult Welcome(string appName, string appSlug, string accent, SsoValidationResult result)
    {
        var html = BuildPage(appName, appSlug, accent, $$"""
            <section class="card success">
              <div class="badge">SSO OK</div>
              <h1>Welcome to {{HtmlEncode(appName)}}</h1>
              <p>Your identity was validated by the Dashboard SSO service.</p>

              <dl class="ident">
                <dt>Full name</dt><dd>{{HtmlEncode(result.FullName)}}</dd>
                <dt>Email</dt><dd>{{HtmlEncode(result.Email)}}</dd>
                <dt>User&nbsp;id</dt><dd>{{result.UserId}}</dd>
                <dt>Tenant&nbsp;id</dt><dd>{{result.TenantId}}</dd>
                <dt>Role</dt><dd><span class="pill">{{HtmlEncode(result.Role)}}</span></dd>
              </dl>

              <p class="muted small">This token was single-use. Reloading the page will fail — return to the Dashboard to get a fresh token.</p>
            </section>
            """);
        return new ContentResult { Content = html, ContentType = "text/html", StatusCode = 200 };
    }

    public static IActionResult Denied(string appName, string appSlug, string accent, string reason)
    {
        var html = BuildPage(appName, appSlug, accent, $$"""
            <section class="card error">
              <div class="badge bad">SSO denied</div>
              <h1>Access denied</h1>
              <p>The SSO token could not be validated.</p>
              <p class="muted">Reason: {{HtmlEncode(reason)}}</p>
              <p class="muted small">Return to the Dashboard and click the tile again — SSO tokens are one-time use and expire after 5 minutes.</p>
            </section>
            """);
        return new ContentResult { Content = html, ContentType = "text/html", StatusCode = 200 };
    }

    private static string BuildPage(string appName, string appSlug, string accent, string bodyContent)
    {
        var accentCss = HtmlEncode(accent);
        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <title>{{HtmlEncode(appName)}}</title>
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <style>
                * { box-sizing: border-box; }
                body {
                  margin: 0;
                  font-family: system-ui, -apple-system, Segoe UI, Roboto, sans-serif;
                  background: #f5f6fa;
                  color: #202124;
                }
                header {
                  background: {{accentCss}};
                  color: white;
                  padding: 18px 32px;
                  display: flex;
                  align-items: center;
                  gap: 12px;
                }
                header h2 { margin: 0; font-size: 18px; letter-spacing: 0.5px; }
                main {
                  max-width: 640px;
                  margin: 40px auto;
                  padding: 0 16px;
                }
                .card {
                  background: white;
                  border-radius: 10px;
                  padding: 28px 32px;
                  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
                }
                .card h1 { margin: 0 0 6px; font-size: 24px; }
                .card p { color: #444; line-height: 1.55; }
                .muted { color: #5f6368; }
                .small { font-size: 12px; }
                .badge {
                  display: inline-block;
                  background: #e6f4ea;
                  color: #137333;
                  padding: 3px 10px;
                  border-radius: 99px;
                  font-size: 12px;
                  margin-bottom: 8px;
                }
                .badge.bad { background: #fce8e6; color: #c5221f; }
                dl.ident {
                  display: grid;
                  grid-template-columns: 120px 1fr;
                  gap: 8px 16px;
                  margin: 20px 0;
                  padding: 16px;
                  background: #f8fafc;
                  border-radius: 8px;
                  font-size: 14px;
                }
                dl.ident dt { color: #5f6368; font-weight: 500; }
                dl.ident dd { margin: 0; color: #202124; }
                .pill {
                  background: #e8f0fe;
                  color: #1a73e8;
                  padding: 2px 10px;
                  border-radius: 10px;
                  font-size: 12px;
                }
                code {
                  background: #f1f3f4;
                  padding: 2px 6px;
                  border-radius: 4px;
                  font-size: 12px;
                }
              </style>
            </head>
            <body>
              <header>
                <strong>{{HtmlEncode(appName)}}</strong>
                <span style="opacity: 0.7; font-size: 12px;">sample app — /{{HtmlEncode(appSlug)}}</span>
              </header>
              <main>
                {{bodyContent}}
              </main>
            </body>
            </html>
            """;
    }

    private static string HtmlEncode(string? value) => WebUtility.HtmlEncode(value ?? "");
}
