using CoreAppwithSSO.ElectionTracker.Extension;
using CoreAppwithSSO.ElectionTracker.Handler;
using CoreAppwithSSO.ElectionTracker.Middleware;
using CoreAppwithSSO.ElectionTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// HttpClient pointed at the central SSO/Dashboard API. The validator uses it to verify
// one-time tokens and poll session status server-side — the browser never talks to the
// SSO API directly, and only ever holds an opaque, HttpOnly session-id cookie.

builder.Services.AddHttpClient<ISsoValidator, SsoValidator>(client =>
{
    var baseUrl = builder.Configuration["Dashboard:ApiBaseUrl"] ?? "http://localhost:5213";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
});

// CORS for the Angular front-end. Credentials must be allowed so the session cookie flows
// when the SPA calls this API cross-origin (the dev-server proxy makes it same-origin, but
// this keeps direct calls working too).
const string SpaCors = "SpaCors";
var spaOrigin = builder.Configuration["Cors:Origin"] ?? "http://localhost:4210";
builder.Services.AddCors(options =>
{
    options.AddPolicy(SpaCors, policy => policy
        .WithOrigins(spaOrigin)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// Business services, Dapper/tenant access, ISC proxies and Swagger generation.
builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

// Swagger first so the UI is reachable at /swagger (launchSettings opens swagger/index.html).
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(SpaCors);

// Global error handling, then per-request authentication + tenant DB routing from the
// validated SSO session (see SessionTenantMiddleware).
app.UseMiddleware<GlobalExceptionHandler>();
app.UseMiddleware<SessionTenantMiddleware>();

app.MapControllers();
app.Run();
