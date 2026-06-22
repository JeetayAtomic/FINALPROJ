using System.Text.Json.Serialization;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Response
{
    public class VoterFilterResponse : ISearchResult<VoterListResponse>
    {
        public int TotalRecord { get; set; }
        public List<VoterListResponse> Results { get; set; } = [];
    }

    public class VoterListResponse : VoterResponse, IAdditionalAttributes
    {
        [JsonIgnore]
        public int TotalRecord { get; set; }
    }
}
