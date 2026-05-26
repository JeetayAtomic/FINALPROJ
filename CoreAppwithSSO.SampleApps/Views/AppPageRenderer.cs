using System.Net;
using System.Text;
using CoreAppwithSSO.SampleApps.Controllers;
using CoreAppwithSSO.SampleApps.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.SampleApps.Views;

/// <summary>HTML renderer for the sample apps — landing, dashboard, and SSO-denied pages.</summary>
public static class AppPageRenderer
{
    public static IActionResult Landing(string appName, string appSlug, string accent, string loginUrl)
    {
        var body = $$"""
            <section class="card landing">
              <h1>{{HtmlEncode(appName)}}</h1>
              <p class="muted">You're not signed in to {{HtmlEncode(appName)}}.</p>
              <p>Sign in with your <strong>SSO account</strong> to continue. After authenticating you'll be returned to the Dashboard where you can launch HR, Finance, or Inventory.</p>
              <a class="signin-btn" href="{{HtmlEncode(loginUrl)}}">Sign in</a>
              <p class="muted small">Already signed in elsewhere? The same Sign-in flow recognises your existing session and won't ask for credentials again.</p>
              <div class="cross-nav">
                <a class="nav-link" href="http://localhost:5301/hr">HR</a>
                <a class="nav-link" href="http://localhost:5302/finance">Finance</a>
                <a class="nav-link" href="http://localhost:5303/inventory">Inventory</a>
              </div>
            </section>
            """;
        return Html(BuildPage(appName, appSlug, accent, body, null, loginUrl, includePollingScript: false));
    }

    public static IActionResult Dashboard(
        string appName,
        string appSlug,
        string accent,
        IReadOnlyList<DashboardTile> tiles,
        SsoSessionStatus session,
        string loginUrl)
    {
        var tilesHtml = new StringBuilder();
        foreach (var t in tiles)
        {
            tilesHtml.Append($$"""
                <div class="tile">
                  <div class="tile-icon">{{HtmlEncode(t.Icon)}}</div>
                  <div class="tile-body">
                    <div class="tile-title">{{HtmlEncode(t.Title)}}</div>
                    <div class="tile-subtitle">{{HtmlEncode(t.Subtitle)}}</div>
                  </div>
                  <div class="tile-metric">{{HtmlEncode(t.Metric)}}</div>
                </div>
                """);
        }

        var body = $$"""
            <section class="dashboard">
              <div class="dashboard-head">
                <div>
                  <h1>{{HtmlEncode(appName)}} Dashboard</h1>
                  <p class="muted">Welcome back, <strong>{{HtmlEncode(session.FullName)}}</strong> — signed in as <span class="pill">{{HtmlEncode(session.Role)}}</span></p>
                </div>
                <div class="dashboard-meta">
                  <div><span class="muted small">User</span> {{HtmlEncode(session.Email)}}</div>
                  <div><span class="muted small">Tenant</span> #{{session.TenantId}}</div>
                  <div><span class="muted small">Session</span> #{{session.SessionId}}</div>
                </div>
              </div>

              <div class="tile-grid">
                {{tilesHtml}}
              </div>

              <p class="muted small foot">SSO session shared across HR, Finance, and Inventory. Logging out signs you out of every app.</p>
            </section>
            """;

        return Html(BuildPage(appName, appSlug, accent, body, session, loginUrl, includePollingScript: true));
    }

    public static IActionResult Denied(string appName, string appSlug, string accent, string reason, string loginUrl)
    {
        var body = $$"""
            <section class="card error">
              <div class="badge bad">SSO denied</div>
              <h1>Access denied</h1>
              <p>The SSO token could not be validated.</p>
              <p class="muted">Reason: {{HtmlEncode(reason)}}</p>
              <a class="signin-btn" href="{{HtmlEncode(loginUrl)}}">Sign in again</a>
              <p class="muted small">SSO tokens are one-time use and expire after 5 minutes — sign in and launch the app again.</p>
            </section>
            """;
        return Html(BuildPage(appName, appSlug, accent, body, null, loginUrl, includePollingScript: false));
    }

    private static IActionResult Html(string content) =>
        new ContentResult { Content = content, ContentType = "text/html", StatusCode = 200 };

    private static string BuildPage(
        string appName,
        string appSlug,
        string accent,
        string bodyContent,
        SsoSessionStatus? session,
        string loginUrl,
        bool includePollingScript)
    {
        var loginUrlJs = HtmlEncode(loginUrl);
        var accentCss = HtmlEncode(accent);
        var headerRight = session != null
            ? $$"""
                <nav class="topnav">
                  <div class="apps-launcher">
                    <button type="button" class="apps-btn" aria-label="Apps" title="Apps">
                      <svg viewBox="0 0 24 24" width="22" height="22" fill="currentColor" aria-hidden="true">
                        <circle cx="5"  cy="5"  r="2"/><circle cx="12" cy="5"  r="2"/><circle cx="19" cy="5"  r="2"/>
                        <circle cx="5"  cy="12" r="2"/><circle cx="12" cy="12" r="2"/><circle cx="19" cy="12" r="2"/>
                        <circle cx="5"  cy="19" r="2"/><circle cx="12" cy="19" r="2"/><circle cx="19" cy="19" r="2"/>
                      </svg>
                    </button>
                    <div class="apps-popover" role="menu">
                      <div class="apps-popover-title">Applications</div>
                      <div class="apps-grid">
                        <a class="app-tile{{(appSlug == "hr" ? " current-app" : "")}}" data-window-name="app_hr" target="app_hr" href="http://localhost:5301/hr">
                          <div class="app-tile-icon" style="background:#4285F4">HR</div>
                          <div class="app-tile-name">HR Portal</div>
                        </a>
                        <a class="app-tile{{(appSlug == "finance" ? " current-app" : "")}}" data-window-name="app_finance" target="app_finance" href="http://localhost:5302/finance">
                          <div class="app-tile-icon" style="background:#EA4335">$</div>
                          <div class="app-tile-name">Finance</div>
                        </a>
                        <a class="app-tile{{(appSlug == "inventory" ? " current-app" : "")}}" data-window-name="app_inventory" target="app_inventory" href="http://localhost:5303/inventory">
                          <div class="app-tile-icon" style="background:#FF6D01">IN</div>
                          <div class="app-tile-name">Inventory</div>
                        </a>
                        <a class="app-tile" data-window-name="app_dashboard" target="app_dashboard" href="http://localhost:4200/dashboard">
                          <div class="app-tile-icon" style="background:#34A853">⌂</div>
                          <div class="app-tile-name">Dashboard</div>
                        </a>
                      </div>
                    </div>
                  </div>
                  <form method="post" action="/{{HtmlEncode(appSlug)}}/logout" class="logout-form">
                    <button type="submit" class="logout-btn" title="Sign out of every app">Log out</button>
                  </form>
                </nav>
                """
            : "";

        // Toggle the 9-dots popover; close on outside click or Esc.
        // App tile clicks reuse an existing tab (by named target) so we don't open duplicates.
        var appsScript = session != null
            ? """
                <script>
                  (function() {
                    var launcher = document.querySelector('.apps-launcher');
                    if (!launcher) return;
                    var btn = launcher.querySelector('.apps-btn');
                    var pop = launcher.querySelector('.apps-popover');

                    btn.addEventListener('click', function(e) {
                      e.stopPropagation();
                      pop.classList.toggle('open');
                    });
                    document.addEventListener('click', function(e) {
                      if (!launcher.contains(e.target)) pop.classList.remove('open');
                    });
                    document.addEventListener('keydown', function(e) {
                      if (e.key === 'Escape') pop.classList.remove('open');
                    });

                    // If a tab with this window-name is already open, focus it instead of
                    // navigating it again (which would reload). Otherwise open a new one.
                    launcher.querySelectorAll('.app-tile').forEach(function(tile) {
                      tile.addEventListener('click', function(e) {
                        e.preventDefault();
                        pop.classList.remove('open');
                        // Clicking the tile for the app we're already on: do nothing.
                        if (tile.classList.contains('current-app')) return;

                        var url  = tile.getAttribute('href');
                        var name = tile.getAttribute('data-window-name');
                        var w    = window.open('', name);
                        if (!w) { window.open(url, name); return; }
                        try {
                          // about:blank means we just created a fresh tab — load the app URL.
                          // Any other URL means a tab with this name already exists; leave it as-is.
                          var href = w.location.href;
                          if (!href || href === 'about:blank') {
                            w.location.href = url;
                          }
                        } catch (_) {
                          // Cross-origin: existing tab on a different domain — just focus it.
                        }
                        try { w.focus(); } catch (_) {}
                      });
                    });
                  })();
                </script>
                """
            : "";

        var pollingScript = includePollingScript
            ? $$"""
                <script>
                  // Detect logout-from-another-app. When the API reports the session as inactive,
                  // bounce the browser straight to the central SSO login page.
                  (function() {
                    var statusUrl = "/{{HtmlEncode(appSlug)}}/session-status";
                    var loginUrl  = "{{loginUrlJs}}";
                    function check() {
                      fetch(statusUrl, { credentials: "same-origin", cache: "no-store" })
                        .then(function(r) { return r.ok ? r.json() : { active: false }; })
                        .then(function(s) { if (!s.active) { window.location.href = loginUrl; } })
                        .catch(function() { /* ignore transient errors */ });
                    }
                    setInterval(check, 5000);
                  })();
                </script>
                """
            : "";

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
                header.app-header {
                  background: {{accentCss}};
                  color: white;
                  padding: 14px 32px;
                  display: flex;
                  align-items: center;
                  gap: 16px;
                  justify-content: space-between;
                }
                header.app-header .brand { display: flex; align-items: baseline; gap: 10px; }
                header.app-header h2 { margin: 0; font-size: 18px; letter-spacing: 0.4px; }
                header.app-header .slug { opacity: 0.75; font-size: 12px; }
                nav.topnav { display: flex; align-items: center; gap: 8px; }
                nav.topnav .nav-link {
                  color: white;
                  text-decoration: none;
                  padding: 6px 12px;
                  border-radius: 999px;
                  font-size: 13px;
                  opacity: 0.85;
                  transition: background 0.15s, opacity 0.15s;
                }
                nav.topnav .nav-link:hover { background: rgba(255,255,255,0.15); opacity: 1; }
                nav.topnav .nav-link.active { background: rgba(255,255,255,0.22); opacity: 1; font-weight: 600; }
                .logout-form { margin: 0 0 0 8px; }
                .logout-btn {
                  background: rgba(0,0,0,0.18);
                  color: white;
                  border: 1px solid rgba(255,255,255,0.35);
                  padding: 6px 14px;
                  border-radius: 999px;
                  font-size: 13px;
                  cursor: pointer;
                  transition: background 0.15s;
                }
                .logout-btn:hover { background: rgba(0,0,0,0.32); }

                .apps-launcher { position: relative; }
                .apps-btn {
                  background: transparent;
                  border: none;
                  color: white;
                  width: 40px;
                  height: 40px;
                  border-radius: 50%;
                  display: inline-flex;
                  align-items: center;
                  justify-content: center;
                  cursor: pointer;
                  opacity: 0.9;
                  transition: background 0.15s, opacity 0.15s;
                }
                .apps-btn:hover { background: rgba(255,255,255,0.18); opacity: 1; }
                .apps-popover {
                  position: absolute;
                  top: calc(100% + 8px);
                  right: 0;
                  width: 300px;
                  background: white;
                  color: #202124;
                  border-radius: 12px;
                  box-shadow: 0 6px 20px rgba(0,0,0,0.18), 0 1px 3px rgba(0,0,0,0.1);
                  padding: 14px;
                  display: none;
                  z-index: 1000;
                }
                .apps-popover.open { display: block; }
                .apps-popover::before {
                  content: "";
                  position: absolute;
                  top: -6px;
                  right: 14px;
                  width: 12px;
                  height: 12px;
                  background: white;
                  transform: rotate(45deg);
                  box-shadow: -1px -1px 1px rgba(0,0,0,0.04);
                }
                .apps-popover-title {
                  font-size: 12px;
                  color: #5f6368;
                  letter-spacing: 0.6px;
                  text-transform: uppercase;
                  padding: 4px 6px 10px;
                }
                .apps-grid {
                  display: grid;
                  grid-template-columns: repeat(3, 1fr);
                  gap: 6px;
                }
                .app-tile {
                  display: flex;
                  flex-direction: column;
                  align-items: center;
                  gap: 6px;
                  padding: 12px 6px;
                  border-radius: 8px;
                  text-decoration: none;
                  color: #202124;
                  transition: background 0.15s;
                }
                .app-tile:hover { background: #f1f3f4; }
                .app-tile.current-app { background: #e8f0fe; cursor: default; position: relative; }
                .app-tile.current-app::after {
                  content: "";
                  position: absolute;
                  top: 6px;
                  right: 6px;
                  width: 6px;
                  height: 6px;
                  border-radius: 50%;
                  background: #1a73e8;
                }
                .app-tile-icon {
                  width: 44px;
                  height: 44px;
                  border-radius: 50%;
                  display: inline-flex;
                  align-items: center;
                  justify-content: center;
                  color: white;
                  font-weight: 700;
                  font-size: 14px;
                  letter-spacing: 0.5px;
                }
                .app-tile-name {
                  font-size: 12px;
                  text-align: center;
                  color: #3c4043;
                }

                main { max-width: 1100px; margin: 32px auto; padding: 0 24px; }

                .card {
                  background: white;
                  border-radius: 10px;
                  padding: 28px 32px;
                  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
                  max-width: 640px;
                }
                .card h1 { margin: 0 0 6px; font-size: 24px; }
                .card p { color: #444; line-height: 1.55; }
                .card.landing { text-align: left; }
                .signin-btn {
                  display: inline-block;
                  background: {{accentCss}};
                  color: white;
                  text-decoration: none;
                  padding: 10px 22px;
                  border-radius: 6px;
                  font-size: 15px;
                  font-weight: 600;
                  margin: 14px 0 8px;
                  box-shadow: 0 2px 6px rgba(0,0,0,0.08);
                  transition: transform 0.1s, box-shadow 0.15s, filter 0.15s;
                }
                .signin-btn:hover { filter: brightness(1.08); box-shadow: 0 4px 12px rgba(0,0,0,0.14); transform: translateY(-1px); }
                .cross-nav { margin-top: 18px; display: flex; gap: 10px; }
                .cross-nav .nav-link {
                  display: inline-block;
                  background: #f1f3f4;
                  color: #1a73e8;
                  text-decoration: none;
                  padding: 6px 14px;
                  border-radius: 999px;
                  font-size: 13px;
                }
                .cross-nav .nav-link:hover { background: #e8f0fe; }

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
                .pill {
                  background: #e8f0fe;
                  color: #1a73e8;
                  padding: 2px 10px;
                  border-radius: 10px;
                  font-size: 12px;
                }

                .dashboard-head {
                  display: flex;
                  justify-content: space-between;
                  align-items: flex-start;
                  gap: 24px;
                  background: white;
                  padding: 24px 28px;
                  border-radius: 10px;
                  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
                  margin-bottom: 24px;
                }
                .dashboard-head h1 { margin: 0 0 6px; font-size: 22px; }
                .dashboard-meta { text-align: right; font-size: 13px; color: #202124; line-height: 1.7; }

                .tile-grid {
                  display: grid;
                  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
                  gap: 16px;
                }
                .tile {
                  background: white;
                  border-radius: 10px;
                  padding: 18px 20px;
                  box-shadow: 0 2px 6px rgba(0,0,0,0.05);
                  display: grid;
                  grid-template-columns: 44px 1fr auto;
                  align-items: center;
                  gap: 14px;
                  border-left: 4px solid {{accentCss}};
                  transition: transform 0.15s, box-shadow 0.15s;
                }
                .tile:hover { transform: translateY(-1px); box-shadow: 0 4px 12px rgba(0,0,0,0.08); }
                .tile-icon { font-size: 26px; text-align: center; }
                .tile-title { font-size: 15px; font-weight: 600; color: #202124; }
                .tile-subtitle { font-size: 12px; color: #5f6368; margin-top: 2px; }
                .tile-metric { font-size: 22px; font-weight: 700; color: {{accentCss}}; }

                .foot { margin-top: 24px; text-align: center; }

                code {
                  background: #f1f3f4;
                  padding: 2px 6px;
                  border-radius: 4px;
                  font-size: 12px;
                }
              </style>
            </head>
            <body>
              <header class="app-header">
                <div class="brand">
                  <h2>{{HtmlEncode(appName)}}</h2>
                  <span class="slug">/{{HtmlEncode(appSlug)}}</span>
                </div>
                {{headerRight}}
              </header>
              <main>
                {{bodyContent}}
              </main>
              {{appsScript}}
              {{pollingScript}}
            </body>
            </html>
            """;
    }

    private static string HtmlEncode(string? value) => WebUtility.HtmlEncode(value ?? "");
}
