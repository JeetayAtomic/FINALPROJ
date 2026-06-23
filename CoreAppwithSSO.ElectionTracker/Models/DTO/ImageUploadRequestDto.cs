using CoreAppwithSSO.ElectionTracker.Common;

namespace CoreAppwithSSO.ElectionTracker.Models.DTO
{
    public class ImageUploadRequestDto
    {
        /// <summary>The image file to upload.</summary>
        public IFormFile File { get; set; } = default!;

        /// <summary>Target entity type: Driver, Truck, Trailer, Banner.</summary>
        public ImageType ImageType { get; set; }

        /// <summary>Human-readable name, e.g. "laptop-pro" or "john-doe".</summary>
        public string EntityName { get; set; } = string.Empty;
    }
}
