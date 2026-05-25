namespace CoreAppwithSSO.API.Models
{
    public class AppSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string OperationSource { get; set; } = string.Empty;
        public string TraslationCreationApiUrl { get; set; } = string.Empty;
        public string APIURL { get; set; } = string.Empty;
        public int TaskCancellationTimeMs { get; set; }
        public string SwaggerEnvironment { get; set; } = string.Empty;

        public string CacheControl { get; set; } = string.Empty;
        public string SubscriptionKey { get; set; } = string.Empty;
    }
}
