using System.Text.Json.Serialization;

namespace CoreAppwithSSO.API.DTOs.Client
{
    public class SealInfoResponse
    {
        [JsonPropertyName("seal_id")]
        public int Seal_Id { get; set; }
        [JsonPropertyName("seal_number")]
        public string? Seal_Number { get; set; }
        [JsonPropertyName("date_sealed")]
        public DateTime Date_Sealed { get; set; }
        [JsonPropertyName("sealed_user")]
        public string? Sealed_User { get; set; }
        [JsonPropertyName("bill_number")]
        public string? Bill_Number { get; set; }
    }
}
