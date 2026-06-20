using CoreAppwithSSO.ElectionTracker.Common;
using CoreAppwithSSO.ElectionTracker.Extension;
using CoreAppwithSSO.ElectionTracker.Models;
using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CoreAppwithSSO.ElectionTracker.Handler
{
    public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger, IOptions<AppSettings> appSettings)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;
        private readonly AppSettings _appSettings = appSettings.Value;

        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Allow the request body to be read again later (in HandleExceptionAsync). The body is a
            // forward-only stream that model binding consumes once; buffering here — before _next
            // runs — lets us rewind and capture the original payload when an exception is thrown.
            context.Request.EnableBuffering();

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var response = await HandleExceptionAsync(context, ex);
                context.Response.StatusCode = response.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response.Body);
            }
        }

        /// <summary>
        /// Handle Exception
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private async Task<(int StatusCode, ATMResponse<object> Body)> HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var loggingService = context.RequestServices.GetRequiredService<ILoggingService>();
            var errorCodeFromContext = context.Items["error"]?.ToString() ?? string.Empty;
            var requestBody = await ReadRequestBodyAsync(context.Request);

            ErrorDescription errorDescription;
            string? responseCode;
            int statusCode;

            switch (ex)
            {
                case BadRequestException badEx:
                    // Explicit business validation error — safe to surface to the caller.
                    errorDescription = new ErrorDescription
                    {
                        ErrorCode = badEx.ErrorCode,
                        ErrorMessage = badEx.Message
                    };
                    responseCode = string.IsNullOrEmpty(errorCodeFromContext) ? badEx.ErrorCode : errorCodeFromContext;
                    statusCode = StatusCodes.Status400BadRequest;
                    break;

                case SqlException sqlEx when TryParseSqlError(sqlEx.Message, out var parsedError, out var parsedResponseCode):
                    // Business rule raised by a stored procedure as structured JSON — safe to surface.
                    errorDescription = parsedError;
                    responseCode = parsedResponseCode;
                    statusCode = StatusCodes.Status400BadRequest;
                    break;

                default:
                    // Genuine/unexpected failure (including infrastructure SqlExceptions such as
                    // timeouts or deadlocks): return a generic message so internal details are not
                    // leaked to the caller. Full details are still logged server-side below.
                    errorDescription = new ErrorDescription
                    {
                        ErrorCode = "UNEXPECTED_ERROR",
                        ErrorMessage = "An unexpected error occurred while processing your request."
                    };
                    responseCode = string.IsNullOrEmpty(errorCodeFromContext) ? "UNEXPECTED_ERROR" : errorCodeFromContext;
                    statusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            var response = new ATMResponse<object>
            {
                IsError = true,
                ResponseCode = responseCode ?? errorDescription.ErrorCode ?? "UNKNOWN",
                Errors = [errorDescription]
            };

            var logErrorOption = new CustomErrorExtensionDto
            {
                Error = new CustomErrorDto
                {
                    LogType = LogType.Error,
                    FunctionName = context.GetEndpoint()?.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()?.ActionName ?? "UnknownAction",
                    ClassName = context.GetEndpoint()?.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()?.ControllerName ?? "UnknownController",
                    ModuleCode = GetModuleCode(context),
                    IPInformation = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP",
                    UserId = context.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "Anonymous",
                },
                Message = ex.Message,
                LogDate = DateTime.UtcNow,
                Source = context.GetEndpoint()?.DisplayName ?? ex.Source ?? "UnKnown Endpoint",
                TargetSite = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                StackTrace = ex.StackTrace ?? "No stack trace available",
                JsonData = string.IsNullOrWhiteSpace(requestBody) ? JsonSerializer.Serialize(response.Data) : requestBody,
                LogTable = _appSettings.ErrorLogTableName,
            };

            await loggingService.LogCustomError(logErrorOption);
            return (statusCode, response);
        }

        /// <summary>
        /// Reads the (already-buffered) request body and rewinds the stream so any downstream
        /// reader is unaffected. Relies on EnableBuffering() having been called in InvokeAsync;
        /// returns empty if the stream is not seekable or has no content.
        /// </summary>
        private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            if (request.Body is null || !request.Body.CanSeek)
            {
                return string.Empty;
            }

            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private bool TryParseSqlError(string message, out ErrorDescription error, out string? responseCode)
        {
            error = null!;
            responseCode = null;

            try
            {
                int endIndex = message.LastIndexOf('}');
                string cleanJson = message[..(endIndex + 1)];
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(cleanJson);
                if (dict is null || !dict.ContainsKey("errorCode")) return false;

                error = new ErrorDescription
                {
                    ErrorCode = dict["errorCode"],
                    ErrorMessage = dict["errorMessage"] ?? "SQL Error"
                };

                responseCode = dict.TryGetValue("reasonCode", out var rc) ? rc : null;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get Module Code
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetModuleCode(HttpContext context)
        {
            var controllerName = context.GetRouteData().Values["controller"]?.ToString() ?? "UnknownController";
            var actionName = context.GetRouteData().Values["action"]?.ToString() ?? "UnknownAction";
            return $"{controllerName}_{actionName}";
        }
    }
}
