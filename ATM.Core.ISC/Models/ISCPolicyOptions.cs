namespace ATM.Core.ISC.Models;

public class ISCPolicyOptions
{
    public int RetryCount { get; set; } = 2;
    public int TimeoutSeconds { get; set; } = 90;

    [Obsolete("Replaced by the proportional breaker. Use CircuitBreakerFailureThreshold, CircuitBreakerSamplingDurationSeconds, and CircuitBreakerMinimumThroughput.")]
    public int CircuitBreakerFailures { get; set; } = 5;

    public int CircuitBreakerDurationSeconds { get; set; } = 20;

    public double CircuitBreakerFailureThreshold { get; set; } = 0.7;

    public int CircuitBreakerSamplingDurationSeconds { get; set; } = 30;

    public int CircuitBreakerMinimumThroughput { get; set; } = 10;
}
