using CoreAppwithSSO.ElectionTracker.Models;
using CoreAppwithSSO.ElectionTracker.Models.DTO;

namespace CoreAppwithSSO.ElectionTracker.Services.Interface
{
    public interface ILoggingService
    {
        Task LogEntryAsync(LogRequestDto logRequest);
        Task LogInfo(LogRequestDto logRequest);
        Task LogWarning(LogRequestDto logRequest);
        Task LogDebug(LogRequestDto logRequest);
        Task LogError(LogRequestDto logRequest);
        Task LogException(Exception exception);
        Task LogErrorDetails(ATMErrorDto logErrorOption);
        Task LogCustomError(CustomErrorExtensionDto logErrorOption);
    }
}
