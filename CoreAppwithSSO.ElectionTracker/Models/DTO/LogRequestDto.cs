using System.ComponentModel.DataAnnotations;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO
{
    public class LogRequestDto
    {
        [DataType(DataType.DateTime)]
        public DateTime? CreatedDate { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(maximumLength: 255, MinimumLength = 5)]
        public string MachineName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string Level { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [StringLength(maximumLength: 255, MinimumLength = 5)]
        public string Logger { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        public string Message { get; set; } = string.Empty;

        public string Exception { get; set; } = string.Empty;
    }
}
