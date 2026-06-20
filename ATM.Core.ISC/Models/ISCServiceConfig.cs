namespace ATM.Core.ISC.Models;

public class ISCServiceConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 100;
}
