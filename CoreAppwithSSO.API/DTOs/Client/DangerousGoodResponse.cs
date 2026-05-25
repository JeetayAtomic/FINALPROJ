using System.Text.Json.Serialization;

namespace CoreAppwithSSO.API.DTOs.Client
{
    public class DangerousGoodResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("display_name")]
        public string? Display_Name { get; set; }
        [JsonPropertyName("un_number")]
        public string? Un_Number { get; set; }
    }
}
