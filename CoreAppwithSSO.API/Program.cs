using CoreAppwithSSO.API.Authentication;
using CoreAppwithSSO.API.Data;
using CoreAppwithSSO.API.Models;
using CoreAppwithSSO.API.OpenApi;
using CoreAppwithSSO.API.Services;
using CoreAppwithSSO.API.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// ---- Databases ----------------------------------------------------------
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

builder.Services.AddDbContext<SsoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SsoConnection")));

// Tenant DB — physical DB resolved per-request from the JWT 'tenant_id' claim.
builder.Services.AddScoped<TenantDbContext>(sp =>
{
    var tenantContext = sp.GetRequiredService<ITenantContext>();
    if (tenantContext.TenantId == null || tenantContext.TenantDbName == null)
        throw new InvalidOperationException(
            "Tenant context not set. This endpoint requires a tenant-scoped JWT.");

    var resolver = sp.GetRequiredService<ITenantConnectionResolver>();
    var connStr = resolver.Resolve(tenantContext.TenantDbName);

    var options = new DbContextOptionsBuilder<TenantDbContext>()
        .UseSqlServer(connStr)
        .Options;

    return new TenantDbContext(options);
});

var appSettingsSection = builder.Configuration.GetSection("AppSettings");
var appSettings = appSettingsSection.Get<AppSettings>() ?? new AppSettings();
builder.Services.Configure<AppSettings>(appSettingsSection);

// ---- Tenancy + supporting services --------------------------------------
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddSingleton<ITenantConnectionResolver, TenantConnectionResolver>();
builder.Services.AddScoped<ITenantProvisioner, TenantProvisioner>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<SuperAdminSeeder>();
builder.Services.AddSingleton<IApiKeyService, ApiKeyService>();

// Data Protection keys: persist to SQL (IdentityDbContext) so the key ring survives
// app pool recycles, redeploys, and is shared across instances behind a load balancer.
// SetApplicationName MUST be stable — changing it invalidates every existing payload.
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<IdentityDbContext>()
    .SetApplicationName("CoreAppwithSSO.API");

// ---- AuthN ---------------------------------------------------------------
// Two schemes co-exist:
//   - JWT Bearer (for interactive users and super admins)
//   - ApiKey (for machine-to-machine access via X-Api-Key header)
// A forwarding policy scheme ("Multi") picks the right handler per request by sniffing the header.
const string MultiScheme = "Multi";
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = MultiScheme;
    options.DefaultAuthenticateScheme = MultiScheme;
    options.DefaultChallengeScheme = MultiScheme;
})
.AddPolicyScheme(MultiScheme, MultiScheme, options =>
{
    options.ForwardDefaultSelector = ctx =>
        ctx.Request.Headers.ContainsKey(ApiKeyAuthenticationOptions.HeaderName)
            ? ApiKeyAuthenticationOptions.DefaultScheme
            : JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true
    };
})
.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
    ApiKeyAuthenticationOptions.DefaultScheme, _ => { });

// ---- AuthZ ---------------------------------------------------------------
// Default: tenant-scoped JWTs only.
// "InterimAuth": the short-lived token issued by /auth/login before tenant selection.
// "SuperAdminOnly": tenant-less JWT with is_super_admin=true.
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim(JwtService.TokenUseClaim, JwtService.TokenUseTenant)
        .RequireClaim(TenantContextMiddleware.TenantIdClaim)
        .Build();

    options.AddPolicy("InterimAuth", policy => policy
        .RequireAuthenticatedUser()
        .RequireClaim(JwtService.TokenUseClaim, JwtService.TokenUseInterim));

    options.AddPolicy("SuperAdminOnly", policy => policy
        .RequireAuthenticatedUser()
        .RequireClaim(JwtService.TokenUseClaim, JwtService.TokenUseSuperAdmin)
        .RequireClaim(JwtService.IsSuperAdminClaim, "true"));

    options.AddPolicy("TenantAdminOnly", policy => policy
        .RequireAuthenticatedUser()
        .RequireClaim(JwtService.TokenUseClaim, JwtService.TokenUseTenant)
        .RequireClaim(TenantContextMiddleware.TenantIdClaim)
        .RequireRole("Admin"));

    // Accepts either a SuperAdmin JWT or a valid X-Api-Key. Endpoints using this
    // policy must still enforce tenant scoping in code when the caller is an API key.
    options.AddPolicy("SuperAdminOrApiKey", policy => policy
        .RequireAuthenticatedUser()
        .RequireAssertion(ctx =>
            (ctx.User.HasClaim(JwtService.TokenUseClaim, JwtService.TokenUseSuperAdmin)
             && ctx.User.HasClaim(JwtService.IsSuperAdminClaim, "true"))
            || ctx.User.IsInRole(ApiKeyAuthenticationHandler.ApiKeyRole)));
});

// ---- CORS + controllers + OpenAPI ---------------------------------------
//var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
//    ?? new[] { "http://localhost:4200", "http://localhost:4201", "http://localhost:4202", "http://localhost:4203" };
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAngular", policy =>
//    {
//        policy.WithOrigins(corsOrigins)
//            .AllowAnyHeader()
//            .AllowAnyMethod()
//            .AllowCredentials();
//    });
//});

var CorsPolicy = "CorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CoreAppwithSSO API",
        Version = "v1",
        Description = "Multi-tenant application dashboard API. Authenticate via /auth/login, then use the Bearer token in the Authorize dialog."
    });
    options.DocumentFilter<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

// ---- Startup: migrate shared DBs + seed super admin + migrate tenant DBs ----
using (var scope = app.Services.CreateScope())
{
    var identity = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    identity.Database.Migrate();

    var sso = scope.ServiceProvider.GetRequiredService<SsoDbContext>();
    sso.Database.Migrate();

    var seeder = scope.ServiceProvider.GetRequiredService<SuperAdminSeeder>();
    await seeder.SeedAsync();

    // Apply pending tenant-DB migrations to every existing tenant so new tables
    // (e.g. JsonTemplates) propagate without requiring tenant re-provisioning.
    var resolver = scope.ServiceProvider.GetRequiredService<ITenantConnectionResolver>();
    var tenantDbNames = await identity.Tenants
        .Where(t => t.IsActive)
        .Select(t => t.DbName)
        .ToListAsync();
    foreach (var dbName in tenantDbNames)
    {
        var connStr = resolver.Resolve(dbName);
        var opts = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer(connStr)
            .Options;
        await using var tenantDb = new TenantDbContext(opts);
        await tenantDb.Database.MigrateAsync();
    }
}

//app.UseSwagger(c =>
//{
//    c.RouteTemplate = "openapi/{documentName}.json";
//});
//app.UseSwaggerUI(options =>
//{
//    options.SwaggerEndpoint("/openapi/v1.json", "CoreAppwithSSO API v1");

//    options.RoutePrefix = "swagger";
//    options.DocumentTitle = "CoreAppwithSSO API — Swagger";
//    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
//    options.DefaultModelsExpandDepth(-1);
//    options.EnablePersistAuthorization();
//    options.EnableTryItOutByDefault();
//    options.DisplayRequestDuration();
//});


app.UseSwagger();
 
if (string.IsNullOrEmpty(appSettings.SwaggerEnvironment))
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "CoreAppwithSSO API — Swagger");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "CoreAppwithSSO API — Swagger";
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.DefaultModelsExpandDepth(-1);
        options.EnablePersistAuthorization();
        options.EnableTryItOutByDefault();
        options.DisplayRequestDuration();
    });
}
else
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(url: appSettings.SwaggerEnvironment + "/swagger/v1/swagger.json", name: "CoreAppwithSSO API — Swagger");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "CoreAppwithSSO API — Swagger";
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.DefaultModelsExpandDepth(-1);
        options.EnablePersistAuthorization();
        options.EnableTryItOutByDefault();
        options.DisplayRequestDuration();
    });
}

// Make hitting the API root land on Swagger UI.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
