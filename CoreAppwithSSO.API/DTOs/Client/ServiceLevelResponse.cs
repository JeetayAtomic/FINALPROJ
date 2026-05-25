using System.Text.Json.Serialization;

namespace CoreAppwithSSO.API.DTOs.Client
{
    public class ServiceLevelResponse
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }
        [JsonPropertyName("codedesc")]
        public string? CodeDesc { get; set; }
    }
}
