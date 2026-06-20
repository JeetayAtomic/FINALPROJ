using ATM.Core.ISC.Extensions;
using CoreAppwithSSO.ElectionTracker.Common;
using CoreAppwithSSO.ElectionTracker.Data;
using CoreAppwithSSO.ElectionTracker.Handler;
using CoreAppwithSSO.ElectionTracker.Mapper;
using CoreAppwithSSO.ElectionTracker.Middleware;
using CoreAppwithSSO.ElectionTracker.Repository.Implementation;
using CoreAppwithSSO.ElectionTracker.Repository.Interface;
using CoreAppwithSSO.ElectionTracker.Services;
using CoreAppwithSSO.ElectionTracker.Services.Implementation;
using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.OpenApi;

namespace CoreAppwithSSO.ElectionTracker.Extension
{
    public static class ServiceExtensions
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

            services.AddScoped<DapperContext>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // Development-only convenience: an "Authorize" box to enter a tenant id, sent as
                // the X-Tenant-Id header on every call. SessionTenantMiddleware honours it only in
                // Development (skips SSO validation and routes straight to that tenant's DB), so it
                // is inert in production.
                const string devTenantScheme = "DevTenant";
                options.AddSecurityDefinition(devTenantScheme, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = SessionTenantMiddleware.DevTenantHeader,
                    Description = "Development only: tenant id to route DB calls and bypass SSO validation."
                });
                options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
                {
                    { new OpenApiSecuritySchemeReference(devTenantScheme, doc), new List<string>() }
                });
            });
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddISCProxiesFromConfig(configuration);

            // Tenant database routing: TenantId (from the SSO session) -> DbName (central
            // Tenants catalog) -> connection string (template). See SessionTenantMiddleware.
            services.AddSingleton<ITenantConnectionResolver, TenantConnectionResolver>();
            services.AddSingleton<ITenantCatalog, TenantCatalog>();

            services.AddAutoMapper(cfg => cfg.AddProfile<BoothProfile>());
            services.AddScoped<IBoothRepository, BoothRepository>();
            services.AddScoped<IBoothService, BoothService>();
            services.AddTransient<HeaderPropagationHandler>();

            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<ITokenProvider, TokenProvider>();
            services.AddScoped<ILoggingService, LoggingService>();

        }

        public static void RegisterMiddlewares(this WebApplication app)
        {
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSwagger();
            var appSettingsSection = app.Configuration.GetSection("AppSettings");
            var appSettings = appSettingsSection.Get<AppSettings>();

            if (string.IsNullOrEmpty(appSettings?.SwaggerEnvironment))
            {
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: " Dispatch API");
                });
            }
            else
            {
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(url: appSettings.SwaggerEnvironment + "/swagger/v1/swagger.json", name: " Contract API");
                });
            }

            app.UseMiddleware<GlobalExceptionHandler>();
            app.UseMiddleware<SessionTenantMiddleware>();
            app.MapControllers();

        }
    }
}
