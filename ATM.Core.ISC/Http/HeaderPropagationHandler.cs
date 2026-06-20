using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
namespace ATM.Core.ISC.Http;

public class HeaderPropagationHandler(
    IHttpContextAccessor httpContextAccessor,
    ILogger<HeaderPropagationHandler> logger) : DelegatingHandler
{
    private static readonly HashSet<string> AllowedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "CorrelationId",
        "traceparent",
        "tracestate",
        "X-Request-ID",
        "Accept",
        "Accept-Language",
        "x-api-key",
        "x-client-id"
    };

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ctx = httpContextAccessor.HttpContext;

            if (ctx != null)
            {
                foreach (var header in ctx.Request.Headers)
                {
                    if (!AllowedHeaders.Contains(header.Key))
                        continue;

                    if (!request.Headers.Contains(header.Key))
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }
            }

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Header propagation failed.");
        }

        return base.SendAsync(request, cancellationToken);
    }
}

