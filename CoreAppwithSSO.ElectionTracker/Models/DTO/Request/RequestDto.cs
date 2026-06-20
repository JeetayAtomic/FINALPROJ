using static CoreAppwithSSO.ElectionTracker.Models.DTO.ApiUtility;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Request
{
    public class RequestDto
    {
        public ApiType ApiType { get; set; } = ApiType.GET;
        public string Url { get; set; } = string.Empty;
        public object? Data { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public ApiUtility.ContentType ContentType { get; set; } = ApiUtility.ContentType.Json;
    }
}
