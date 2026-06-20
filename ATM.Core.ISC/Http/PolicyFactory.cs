using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace ATM.Core.ISC.Http;

public static class PolicyFactory
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount = 3)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int exceptionsAllowedBeforeBreaking = 5, int durationOfBreakSeconds = 30)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking, TimeSpan.FromSeconds(durationOfBreakSeconds));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetAdvancedCircuitBreakerPolicy(
        double failureThreshold = 0.5,
        int samplingDurationSeconds = 30,
        int minimumThroughput = 8,
        int durationOfBreakSeconds = 30)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: failureThreshold,
                samplingDuration: TimeSpan.FromSeconds(samplingDurationSeconds),
                minimumThroughput: minimumThroughput,
                durationOfBreak: TimeSpan.FromSeconds(durationOfBreakSeconds));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int timeoutSeconds = 30)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(timeoutSeconds));
    }
}
