using ATM.Core.ISC.Abstractions;
using ATM.Core.ISC.Core;
using ATM.Core.ISC.Http;
using ATM.Core.ISC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace ATM.Core.ISC.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddISCProxiesFromConfig(
    this IServiceCollection services,
    IConfiguration configuration,
    Action<ISCPolicyOptions>? policyOptions = null)
    {
        var opts = new ISCPolicyOptions();
        policyOptions?.Invoke(opts);

        services.AddTransient<HeaderPropagationHandler>();
        services.AddTransient<HttpLoggingHandler>();

        // Try ISCServices first
        var iscSection = configuration.GetSection("ISCServices");
        var iscServiceConfigs = iscSection.Get<Dictionary<string, ISCServiceConfig>>();

        // Check to ServiceUrls if ISCServices not found
        var legacyUrlsSection = configuration.GetSection("ServiceUrls");
        var legacyUrls = legacyUrlsSection.Get<Dictionary<string, string>>();

        Dictionary<string, ISCServiceConfig> finalConfigs = [];

        if (iscServiceConfigs != null && iscServiceConfigs.Count > 0)
        {
            finalConfigs = iscServiceConfigs;
        }
        else if (legacyUrls != null && legacyUrls.Count > 0)
        {
            foreach (var kv in legacyUrls)
            {
                finalConfigs[kv.Key] = new ISCServiceConfig
                {
                    BaseUrl = kv.Value,
                    TimeoutSeconds = opts.TimeoutSeconds
                };
            }
        }
        else
        {
            throw new InvalidOperationException("No ISC services or ServiceUrls found in configuration.");
        }

        foreach (var kv in finalConfigs)
        {
            var clientName = kv.Key;
            var cfg = kv.Value;

            services.AddHttpClient(clientName, client =>
            {
                client.BaseAddress = new Uri(EnsureTrailingSlash(cfg.BaseUrl));
                // Polly owns timing via the per-attempt timeout policy below; an HttpClient.Timeout
                // shorter than (perAttemptTimeout * (retryCount + 1) + backoff) would cancel the
                // chain before retries finish.
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<HeaderPropagationHandler>()
            .AddHttpMessageHandler<HttpLoggingHandler>()
            // Outer: proportional breaker. Opens only when the failure RATIO over the sampling
            // window exceeds the threshold AND at least MinimumThroughput calls happened in that
            // window. A flaky vendor with intermittent failures does not trip it; a genuinely
            // unhealthy one does.
            .AddPolicyHandler(PolicyFactory.GetAdvancedCircuitBreakerPolicy(
                opts.CircuitBreakerFailureThreshold,
                opts.CircuitBreakerSamplingDurationSeconds,
                opts.CircuitBreakerMinimumThroughput,
                opts.CircuitBreakerDurationSeconds))
            .AddPolicyHandler(PolicyFactory.GetRetryPolicy(opts.RetryCount))
            // Innermost: per-attempt timeout. A slow attempt is retried instead of hanging the chain.
            .AddPolicyHandler(PolicyFactory.GetTimeoutPolicy(cfg.TimeoutSeconds));
        }

        services.AddHttpContextAccessor();

        var defaultClient = finalConfigs.Keys.First();

        services.AddSingleton<IISCClient>(sp =>
            new ISCClient(
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<ILogger<ISCClient>>(),
                defaultClientName: defaultClient
            )
        );

        return services;
    }

    #region Private Methods
    private static string EnsureTrailingSlash(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException("BaseUrl cannot be null or empty.");

        return url.EndsWith("/") ? url : url + "/";
    }
    #endregion
}
