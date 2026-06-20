using CoreAppwithSSO.ElectionTracker.Controllers;
using CoreAppwithSSO.ElectionTracker.Models;
using CoreAppwithSSO.ElectionTracker.Services;
using CoreAppwithSSO.ElectionTracker.Services.Interface;

namespace CoreAppwithSSO.ElectionTracker.Middleware
{
    /// <summary>
    /// Establishes both the authenticated user and the target tenant database for every data
    /// request, from the opaque <c>et_session_id</c> cookie alone — the client passes nothing
    /// else and cannot choose which tenant DB it talks to.
    ///
    /// Per request: re-validate the session against the SSO API (authoritative source of the
    /// tenant id), look up that tenant's physical DB name in the central catalog, build the
    /// connection from the template, and stamp it onto the scoped <see cref="ITenantService"/>
    /// so <c>DapperContext</c> opens the correct client database.
    /// </summary>
    public class SessionTenantMiddleware
    {
        /// <summary>HttpContext.Items key holding the validated <see cref="SsoSessionStatus"/> for the request.</summary>
        public const string SessionItemKey = "SsoSession";

        /// <summary>Development-only header that selects the tenant directly and skips SSO validation (see Swagger testing).</summary>
        public const string DevTenantHeader = "X-Tenant-Id";

        private readonly RequestDelegate _next;

        public SessionTenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ISsoValidator sso,
            ITenantService tenantService,
            ITenantCatalog tenantCatalog,
            ITenantConnectionResolver connectionResolver,
            IConfiguration config,
            IWebHostEnvironment env)
        {
            // Endpoints that must run before a session exists (token exchange) or that are
            // infrastructure (Swagger) bypass session+tenant resolution entirely.
            if (IsAnonymousPath(context.Request))
            {
                await _next(context);
                return;
            }

            // Development-only escape hatch for Swagger / local testing: supply X-Tenant-Id to
            // route directly to that tenant's DB without an SSO session. NEVER active outside
            // Development, so production stays SSO-session-only.
            if (env.IsDevelopment() &&
                int.TryParse(context.Request.Headers[DevTenantHeader].FirstOrDefault(), out var devTenantId) &&
                devTenantId > 0)
            {
                if (!await TrySetTenantAsync(devTenantId, tenantService, tenantCatalog, connectionResolver))
                {
                    await WriteJsonAsync(context, StatusCodes.Status403Forbidden, "Tenant is not provisioned.");
                    return;
                }

                var expectedAppId = config.GetValue<int>("Sso:ApplicationId");
                var devUserId = int.TryParse(context.Request.Headers["X-Dev-User-Id"].FirstOrDefault(), out var u) ? u : 1;
                context.Items[SessionItemKey] = new SsoSessionStatus(
                    Active: true, SessionId: 0, UserId: devUserId, TenantId: devTenantId,
                    ApplicationId: expectedAppId, Email: "dev@local", FullName: "Dev User", Role: "Admin");

                await _next(context);
                return;
            }

            if (!TryGetSessionId(context, out var sessionId))
            {
                await WriteJsonAsync(context, StatusCodes.Status401Unauthorized, "Not signed in.");
                return;
            }

            var status = await sso.GetSessionAsync(sessionId, context.RequestAborted);
            var expectedApplicationId = config.GetValue<int>("Sso:ApplicationId");
            if (!status.Active || status.ApplicationId != expectedApplicationId)
            {
                await WriteJsonAsync(context, StatusCodes.Status401Unauthorized, "Session is not active for this application.");
                return;
            }

            if (!await TrySetTenantAsync(status.TenantId, tenantService, tenantCatalog, connectionResolver))
            {
                await WriteJsonAsync(context, StatusCodes.Status403Forbidden, "Tenant is not provisioned.");
                return;
            }

            context.Items[SessionItemKey] = status;

            await _next(context);
        }

        /// <summary>
        /// Resolves the tenant's physical database from the central catalog and stamps it onto
        /// the scoped tenant service. Returns false when the tenant is missing/inactive.
        /// </summary>
        private static async Task<bool> TrySetTenantAsync(
            int tenantId,
            ITenantService tenantService,
            ITenantCatalog tenantCatalog,
            ITenantConnectionResolver connectionResolver)
        {
            var dbName = await tenantCatalog.GetDbNameAsync(tenantId);
            if (string.IsNullOrEmpty(dbName))
                return false;

            tenantService.SetCurrentTenant(new Tenant
            {
                Id = tenantId.ToString(),
                ConnectionString = connectionResolver.Resolve(dbName),
                DbProvider = DbProvider.SqlServer
            });
            return true;
        }

        private static bool IsAnonymousPath(HttpRequest request)
        {
            var path = request.Path;
            if (path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
                return true;

            // The SSO callback exchanges a one-time token for a session; no session exists yet.
            if (HttpMethods.IsPost(request.Method) &&
                path.StartsWithSegments("/api/election/sso/callback", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static bool TryGetSessionId(HttpContext context, out int sessionId)
        {
            sessionId = 0;
            return context.Request.Cookies.TryGetValue(ElectionController.SessionCookieName, out var raw)
                   && int.TryParse(raw, out sessionId)
                   && sessionId > 0;
        }

        private static Task WriteJsonAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsJsonAsync(new { message });
        }
    }
}
