namespace CoreAppwithSSO.ElectionTracker.Models.DTO
{
    public class ApiUtility
    {
        public const string TokenCookie = "JWTToken";
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
        public enum ContentType
        {
            Json,
            MultipartFormData,
        }

        public static string LoggingAPIBase { get; set; } = string.Empty;
        public static string CommonAPIBase { get; set; } = string.Empty;
    }
}
