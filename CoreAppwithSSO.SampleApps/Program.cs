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

// Each sample app gets its own URL. Listening port -> slug; controllers gate themselves
// with [Host(...)] so e.g. HR only responds on :5301. Visiting "/" on any port redirects
// straight to that app's dashboard (or its landing if not signed in).
var portToSlug = new Dictionary<int, string>
{
    [5301] = "hr",
    [5302] = "finance",
    [5303] = "inventory"
};

app.MapGet("/", (HttpContext ctx) =>
{
    var port = ctx.Connection.LocalPort;
    if (portToSlug.TryGetValue(port, out var slug))
        return Results.Redirect($"/{slug}");

    // Fallback: list every app with its URL (only reached if the request came in on an unknown port).
    return Results.Content("""
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
          <p class="muted">Each app runs on its own URL. Use the Dashboard tiles to land here with a valid one-time token.</p>
          <ul>
            <li><a href="http://localhost:5301/hr">HR Portal</a> — <code>http://localhost:5301/hr</code></li>
            <li><a href="http://localhost:5302/finance">Finance</a> — <code>http://localhost:5302/finance</code></li>
            <li><a href="http://localhost:5303/inventory">Inventory</a> — <code>http://localhost:5303/inventory</code></li>
          </ul>
        </body>
        </html>
        """, "text/html");
});

app.MapControllers();
app.Run();
