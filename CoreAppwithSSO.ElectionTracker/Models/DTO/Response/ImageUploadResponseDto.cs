namespace CoreAppwithSSO.ElectionTracker.Models.DTO.Response
{
    public class ImageUploadResponseDto
    {
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Relative path on disk, e.g. "images/Product/product_laptop_20240615_abc123.jpg".
        /// Store this in your database — use it to delete or reconstruct the URL later.
        /// </summary>
        public string RelativePath { get; set; } = string.Empty;

        /// <summary>
        /// Filename only, e.g. "product_laptop_20240615_abc123.jpg".
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Absolute URL ready for use in <img src="...">.
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}
