using CoreAppwithSSO.ElectionTracker.Common;
using CoreAppwithSSO.ElectionTracker.Models.Domain;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Response
{
    public class KaryakartaResponse : BaseModel
    {
        public int KaryakartaId { get; set; }
        public string KaryakartaCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Int32 ConstituencyId { get; set; }
        public Int32 BoothId { get; set; }
        public Int32 WardId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; }
        public string Rating { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string KaryakartaPhotoUrl { get; set; } = string.Empty;
        public string? HouseNo { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string? Address1 { get; set; } = string.Empty;
        public string? Caste { get; set; } = string.Empty;
    }
}
