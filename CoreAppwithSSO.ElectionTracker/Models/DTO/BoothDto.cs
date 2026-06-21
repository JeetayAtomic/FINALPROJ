using CoreAppwithSSO.ElectionTracker.Models.Domain;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO
{
    public class BoothDto : BaseModel
    {
        public int BoothId { get; set; }

        // Business identity (originally "ShortId" on the client model).
        public string BoothCode { get; set; } = string.Empty;

        public int WardId { get; set; }
        public string WardName { get; set; } = string.Empty;
        public string WardNeighborhood { get; set; } = string.Empty;
        public string ConstituencyId { get; set; } = string.Empty;
        public string ConstituencyName { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int VoterCount { get; set; }

        // Nested objects stored as JSON (see BoothPerson / BoothMaterials for the shape).
        public BoothPerson Agent { get; set; } = new BoothPerson();
        public string Backup { get; set; } = string.Empty;
        public BoothMaterial Materials { get; set; } = new BoothMaterial();

        public bool Accessibility { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
