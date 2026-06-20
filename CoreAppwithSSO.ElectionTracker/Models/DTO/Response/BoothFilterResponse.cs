using System.Text.Json.Serialization;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Response
{
    public class BoothFilterResponse : ISearchResult<BoothListResponse>
    {
        public int TotalRecord { get; set; }
        public List<BoothListResponse> Results { get; set; } = [];
    }

    public class BoothListResponse : BoothResponse, IAdditionalAttributes
    {
        [JsonIgnore]
        public int TotalRecord { get; set; }
    }
}
