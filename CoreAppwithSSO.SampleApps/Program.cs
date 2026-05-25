using CoreAppwithSSO.SampleApps.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// HttpClient pointed at the Dashboard API (used by ISsoValidator to validate tokens server-side).
builder.Services.AddHttpClient<ISsoValidator, SsoValidator>(client =>
{
    var baseUrl = builder.Configuration["Dashboard:ApiBaseUrl"] ?? "http://localhost:5213";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
});

var app = builder.Build();

// Root page gives a quick overview of the three sample apps.
app.MapGet("/", () => Results.Content("""
    <!doctype html>
    <html lang="en">
    <head><meta charset="utf-8" /><title>Sample Apps</title>
    <style>
      body { font-family: system-ui, sans-serif; max-width: 640px; margin: 60px auto; padding: 0 16px; color: #202124; }
      h1 { font-size: 22px; }
      ul { padding: 0; list-style: none; }
      li { padding: 10px 0; border-bottom: 1px solid #eee; }
      a { color: #1a73e8; text-decoration: none; }
      a:hover { text-decoration: underline; }
      .muted { color: #5f6368; font-size: 13px; }
    </style></head>
    <body>
      <h1>CoreAppwithSSO sample apps</h1>
      <p class="muted">Targets for SSO from the main dashboard. Use the dashboard tiles to land here with a valid one-time token.</p>
      <ul>
        <li><a href="/hr">HR Portal</a> — <code>/hr</code></li>
        <li><a href="/finance">Finance</a> — <code>/finance</code></li>
        <li><a href="/inventory">Inventory</a> — <code>/inventory</code></li>
      </ul>
    </body>
    </html>
    """, "text/html"));

app.MapControllers();
app.Run();
