using CoreAppwithSSO.ElectionTracker.Models.Domain;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Request
{
    public class ConstituencyRequest : BaseModel
    {
        public int ConstituencyId { get; set; }
        public int DistrictId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "AC";
        public Int32 StateId { get; set; } = 0;
        public string Region { get; set; } = string.Empty;
        public string SubRegion { get; set; } = string.Empty;
    }
}
