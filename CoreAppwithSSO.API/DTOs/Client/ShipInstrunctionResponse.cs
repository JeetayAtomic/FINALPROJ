using System.Text.Json.Serialization;

namespace CoreAppwithSSO.API.DTOs.Client
{
    public class ShipInstrunctionResponse
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }
        [JsonPropertyName("label")]
        public string? Label { get; set; }
        [JsonPropertyName("applies_to")]
        public string? Applies_To { get; set; }
    }
}
