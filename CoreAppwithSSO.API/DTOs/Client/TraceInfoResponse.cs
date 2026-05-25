using System.Text.Json.Serialization;

namespace CoreAppwithSSO.API.DTOs.Client
{
    public class TraceInfoResponse
    {
        [JsonPropertyName("trace_number")]
        public string? TraceNumber { get; set; }

        [JsonPropertyName("trace_type")]
        public string? TraceType { get; set; }

        [JsonPropertyName("ref_qualifier")]
        public string? RefQualifier { get; set; }

        [JsonPropertyName("desc")]
        public string? Description { get; set; }
    }
}
