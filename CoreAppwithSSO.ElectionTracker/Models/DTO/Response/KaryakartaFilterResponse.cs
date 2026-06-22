using System.Text.Json.Serialization;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Response
{
    public class KaryakartaFilterResponse : ISearchResult<KaryakartaListResponse>
    {
        public int TotalRecord { get; set; }
        public List<KaryakartaListResponse> Results { get; set; } = [];
    }

    public class KaryakartaListResponse : KaryakartaResponse, IAdditionalAttributes
    {
        [JsonIgnore]
        public int TotalRecord { get; set; }
    }
}
