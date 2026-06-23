using CoreAppwithSSO.ElectionTracker.Common;
using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Services.Interface;

namespace CoreAppwithSSO.ElectionTracker.Services.Implementation
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImageUploadService> _logger;

        // Allowed extensions and max file size (5 MB)
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public ImageUploadService(IWebHostEnvironment env, ILogger<ImageUploadService> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Uploads an image into wwwroot/images/{ImageType}/ with a unique, readable filename.
        /// </summary>
        /// <param name="file">The uploaded file.</param>
        /// <param name="imageType">Enum: Profile, Product, Category, etc.</param>
        /// <param name="entityName">Human-readable name, e.g. "laptop-pro" or "john-doe".</param>
        public async Task<ImageUploadResult> UploadAsync(IFormFile file, ImageType imageType, string entityName)
        {
            // --- Validation ---
            if (file == null || file.Length == 0)
                return Fail("No file provided.");

            if (file.Length > MaxFileSizeBytes)
                return Fail($"File size exceeds the {MaxFileSizeBytes / 1024 / 1024} MB limit.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return Fail($"Extension '{ext}' is not allowed. Use: {string.Join(", ", AllowedExtensions)}");

            // --- Build folder path: wwwroot/images/{ImageType}/ ---
            var folderRelative = Path.Combine("images", imageType.ToString());
            var folderAbsolute = Path.Combine(GetWebRootPath(), folderRelative);
            Directory.CreateDirectory(folderAbsolute); // no-op if already exists

            // --- Build unique readable filename ---
            // Format: {imageType}_{sanitizedEntityName}_{yyyyMMdd}_{shortGuid}{ext}
            // Example: product_laptop-pro_20240615_a3f2b1c9.jpg
            var sanitized = SanitizeName(entityName);
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var uniquePart = Guid.NewGuid().ToString("N")[..8];
            var fileName = $"{imageType.ToString().ToLower()}_{sanitized}_{datePart}_{uniquePart}{ext}";

            var absoluteFilePath = Path.Combine(folderAbsolute, fileName);

            // --- Save file ---
            await using var stream = new FileStream(absoluteFilePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var relativePath = Path.Combine(folderRelative, fileName).Replace("\\", "/");
            _logger.LogInformation("Image uploaded: {Path}", relativePath);

            return new ImageUploadResult
            {
                Success = true,
                RelativePath = relativePath,
                FileName = fileName
            };
        }

        /// <summary>
        /// Deletes an image given its relative path (e.g. "images/Product/product_laptop_20240101_abc.jpg").
        /// </summary>
        public bool Delete(string relativePath)
        {
            var absolutePath = Path.Combine(GetWebRootPath(), relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (!File.Exists(absolutePath)) return false;

            File.Delete(absolutePath);
            _logger.LogInformation("Image deleted: {Path}", relativePath);
            return true;
        }

        // --- Helpers ---

        /// <summary>
        /// Resolves the web root, falling back to {ContentRoot}/wwwroot when WebRootPath is null
        /// (ASP.NET Core leaves it null when the wwwroot folder does not exist at startup) and
        /// ensures the directory exists so uploads/serves work on a fresh checkout or deploy.
        /// </summary>
        private string GetWebRootPath()
        {
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");

            Directory.CreateDirectory(webRoot);
            return webRoot;
        }

        private static ImageUploadResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };

        /// <summary>
        /// Converts entity name to a safe, lowercase, hyphenated string.
        /// "Laptop Pro 15!" → "laptop-pro-15"
        /// </summary>
        private static string SanitizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "unnamed";

            var safe = new string(name
                .ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '-')
                .ToArray());

            // Collapse multiple dashes and trim
            while (safe.Contains("--")) safe = safe.Replace("--", "-");
            return safe.Trim('-')[..Math.Min(safe.Length, 40)]; // cap at 40 chars
        }
    }
}
