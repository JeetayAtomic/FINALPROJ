namespace CoreAppwithSSO.ElectionTracker.Common
{
    public class AppSettings
    {
        public string SwaggerEnvironment { get; set; } = string.Empty;
        public bool EnableSSL { get; set; }
        public string ErrorLogTableName { get; set; } = string.Empty;
    }
}
