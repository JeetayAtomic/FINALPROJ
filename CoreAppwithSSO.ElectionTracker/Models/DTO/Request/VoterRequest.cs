using CoreAppwithSSO.ElectionTracker.Common;
using CoreAppwithSSO.ElectionTracker.Models.Domain;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Request
{
    public class VoterRequest : BaseModel
    {
        public Int64 VoterId { get; set; }
        public string EpicNo { get; set; } = string.Empty;
        public Int32 BoothId { get; set; }
        public string SerialNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public string? RelationName { get; set; } = string.Empty;
        public string? RelationType { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string? HouseNo { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string? Address1 { get; set; } = string.Empty;
        public string? MobileNo { get; set; } = string.Empty;
        public string? Caste { get; set; } = string.Empty;
        public string? SubCaste { get; set; } = string.Empty;
        public string? Religion { get; set; } = string.Empty;
        public string VoterPhotoUrl { get; set; } = string.Empty;
        public string VoterIdCardUrl { get; set; } = string.Empty;
        public bool IsOutsideConstituency { get; set; }
    }
}
