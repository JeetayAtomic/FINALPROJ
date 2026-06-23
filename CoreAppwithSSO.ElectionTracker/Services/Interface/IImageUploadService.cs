using CoreAppwithSSO.ElectionTracker.Common;
using CoreAppwithSSO.ElectionTracker.Models.Domain;

namespace CoreAppwithSSO.ElectionTracker.Services.Interface
{
    public interface IImageUploadService
    {
        Task<ImageUploadResult> UploadAsync(IFormFile file, ImageType imageType, string entityName);
        bool Delete(string relativePath);
    }
}
