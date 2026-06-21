using System.Text.Json.Serialization;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Response
{
    public class ConstituencyFilterResponse : ISearchResult<ConstituencyListResponse>
    {
        public int TotalRecord { get; set; }
        public List<ConstituencyListResponse> Results { get; set; } = [];
    }

    public class ConstituencyListResponse : ConstituencyResponse, IAdditionalAttributes
    {
        [JsonIgnore]
        public int TotalRecord { get; set; }
    }
}
