using System.Text.Json.Serialization;

namespace CoreAppwithSSO.API.DTOs.Client
{
    public class ClientResponse
    {
        [JsonPropertyName("client_id")]
        public string? Client_Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address_1")]
        public string? Address_1 { get; set; }

        [JsonPropertyName("address_2")]
        public string? Address_2 { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("province")]
        public string? Province { get; set; }

        [JsonPropertyName("postal_code")]
        public string? Postal_Code { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("business_phone")]
        public string? Business_Phone { get; set; }

        [JsonPropertyName("business_phone_ext")]
        public string? Business_Phone_Ext { get; set; }

        [JsonPropertyName("fax_phone")]
        public string? Fax_Phone { get; set; }

        [JsonPropertyName("business_cell")]
        public string? Business_Cell { get; set; }

        [JsonPropertyName("contact")]
        public string? Contact { get; set; }

        [JsonPropertyName("default_delivery_z")]
        public string? Default_Delivery_Z { get; set; }
    }
}
