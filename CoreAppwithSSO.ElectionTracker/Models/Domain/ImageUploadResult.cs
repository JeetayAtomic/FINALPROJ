namespace CoreAppwithSSO.ElectionTracker.Models.Domain
{
    public class ImageUploadResult
    {
        public bool Success { get; set; }
        public string? RelativePath { get; set; }   // e.g. "images/Product/product_laptop-pro_20240101_abc123.jpg"
        public string? FileName { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
