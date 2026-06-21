using CoreAppwithSSO.ElectionTracker.Models.Domain;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Request
{
    public class SectorRequest : BaseModel
    {
        public int SectorId { get; set; }
        public int BoothId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
