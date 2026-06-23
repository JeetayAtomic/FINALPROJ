using CoreAppwithSSO.ElectionTracker.Common;
using CoreAppwithSSO.ElectionTracker.Constants;
using CoreAppwithSSO.ElectionTracker.Models.DTO;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;
using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.ElectionTracker.Controllers
{
    [Route(UrlConstant.RouteImageUpload)]
    [Authorize]
    [ApiController]
    public class ImageController : BaseController
    {
        private readonly IImageUploadService _imageService;
        private readonly IWebHostEnvironment _env;

        public ImageController(IImageUploadService imageService, IWebHostEnvironment env)
        {
            _imageService = imageService;
            _env = env;
        }

        /// <summary>
        /// POST api/ImageUpload/upload
        /// Uploads an image for any entity type.
        /// Returns the relative path, filename, and absolute URL wrapped in ATMResponse.
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] ImageUploadRequestDto request)
        {
            this.HttpContext.Items["error"] = Constant.SAVE_IMAGE_ERROR;
            var response = new ATMResponse<ImageUploadResponseDto>
            {
                UserEmail = Email
            };

            var result = await _imageService.UploadAsync(request.File, request.ImageType, request.EntityName);

            if (!result.Success)
            {
                response.IsError = true;
                response.ResponseCode = Constant.SAVE_IMAGE_ERROR;
                response.Errors.Add(new ErrorDescription
                {
                    ErrorCode = Constant.SAVE_IMAGE_ERROR,
                    ErrorMessage = result.ErrorMessage ?? "Upload failed."
                });
                return Ok(response);
            }

            response.ResponseCode = Constant.SAVE_IMAGE_SUCCESS;
            response.Data.Add(new ImageUploadResponseDto
            {
                Message = "Upload successful.",
                RelativePath = result.RelativePath!,
                FileName = result.FileName!,
                Url = $"{Request.Scheme}://{Request.Host}/{result.RelativePath}"
            });

            return Ok(response);
        }

        /// <summary>
        /// GET api/ImageUpload?url=images/Truck/truck_abc_20240615_abc123.jpg
        /// Serves an image file by its relative path.
        /// Returns the raw image bytes with the correct Content-Type header.
        /// Note: binary content is served directly and is intentionally not wrapped in ATMResponse.
        /// </summary>
        [HttpGet(UrlConstant.GetImage)]
        public IActionResult GetImage([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest(new { message = "url is required." });

            // Prevent path traversal attacks (e.g. "../../appsettings.json")
            var normalizedUrl = url.Replace("\\", "/");
            if (normalizedUrl.Contains(".."))
                return BadRequest(new { message = "Invalid path." });

            var webRoot = string.IsNullOrEmpty(_env.WebRootPath)
                ? Path.Combine(_env.ContentRootPath, "wwwroot")
                : _env.WebRootPath;

            var absolutePath = Path.Combine(
                webRoot,
                normalizedUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (!System.IO.File.Exists(absolutePath))
                return NotFound(new { message = "Image not found." });

            // Resolve MIME type from file extension
            var ext = Path.GetExtension(absolutePath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };

            var imageBytes = System.IO.File.ReadAllBytes(absolutePath);
            return File(imageBytes, contentType);
        }

        /// <summary>
        /// DELETE api/ImageUpload
        /// Deletes an uploaded image by its relative path.
        /// </summary>
        [HttpDelete(UrlConstant.DeleteImage)]
        public IActionResult Delete([FromQuery] string relativePath)
        {
            this.HttpContext.Items["error"] = Constant.DELETE_IMAGE_ERROR;
            var response = new ATMResponse<bool>
            {
                UserEmail = Email
            };

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                response.IsError = true;
                response.ResponseCode = Constant.DELETE_IMAGE_ERROR;
                response.Errors.Add(new ErrorDescription
                {
                    ErrorCode = Constant.DELETE_IMAGE_ERROR,
                    ErrorMessage = "relativePath is required."
                });
                return Ok(response);
            }

            var deleted = _imageService.Delete(relativePath);

            response.IsError = !deleted;
            response.ResponseCode = deleted ? Constant.DELETE_IMAGE_SUCCESS : Constant.DELETE_IMAGE_ERROR;
            response.Data.Add(deleted);

            if (!deleted)
                response.Errors.Add(new ErrorDescription
                {
                    ErrorCode = Constant.DELETE_IMAGE_ERROR,
                    ErrorMessage = "Image not found."
                });

            return Ok(response);
        }
    }
}
