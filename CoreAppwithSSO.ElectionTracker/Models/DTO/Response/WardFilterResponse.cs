using System.Text.Json.Serialization;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Response
{
    public class WardFilterResponse : ISearchResult<WardListResponse>
    {
        public int TotalRecord { get; set; }
        public List<WardListResponse> Results { get; set; } = [];
    }

    public class WardListResponse : WardResponse, IAdditionalAttributes
    {
        [JsonIgnore]
        public int TotalRecord { get; set; }
    }
}
