using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
namespace ATM.Core.ISC.Http;

public class HttpLoggingHandler(ILogger<HttpLoggingHandler> _logger) : DelegatingHandler
{

    protected async override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Outgoing HTTP {Method} {Url}", request.Method, request.RequestUri);

            if (request.Content != null)
            {
                var reqBody = await request.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Request Body: {Body}", reqBody);
            }
            var response = await base.SendAsync(request, cancellationToken);

            sw.Stop();
            _logger.LogInformation("Incoming HTTP {Status} from {Url} in {Time}ms",
                response.StatusCode, request.RequestUri, sw.ElapsedMilliseconds);

            if (response.Content != null)
            {
                var respBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Response Body: {Body}", respBody);
            }

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "HTTP call failed for {Url} after {Time}ms",
                request.RequestUri, sw.ElapsedMilliseconds);

            throw;
        }
    }
}