using System.Text.Json.Serialization;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Response
{
    public class SectorFilterResponse : ISearchResult<SectorListResponse>
    {
        public int TotalRecord { get; set; }
        public List<SectorListResponse> Results { get; set; } = [];
    }

    public class SectorListResponse : SectorResponse, IAdditionalAttributes
    {
        [JsonIgnore]
        public int TotalRecord { get; set; }
    }
}
