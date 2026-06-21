using CoreAppwithSSO.ElectionTracker.Models.Domain;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Response
{
    public class WardResponse : BaseModel
    {
        public int WardId { get; set; }
        public string WardCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public int ConstituencyId { get; set; } = 0;
        public int Households { get; set; }
        public string? LeadId { get; set; }
        public int Coverage { get; set; }
        public int SupportLevel { get; set; }
        public int LastCanvass { get; set; }
        public string Flags { get; set; } = string.Empty;
        public int Status { get; set; }
    }
}
