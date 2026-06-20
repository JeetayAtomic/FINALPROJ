using CoreAppwithSSO.ElectionTracker.Models;
using CoreAppwithSSO.ElectionTracker.Models.DTO;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Services.Interface;

namespace CoreAppwithSSO.ElectionTracker.Services.Implementation
{
    public class LoggingService(IBaseService baseService) : ILoggingService
    {
        private readonly string LoggingApiBaseUrl = $"{ApiUtility.LoggingAPIBase}/api/LoggingApi/";

        public async Task LogEntryAsync(LogRequestDto logRequest)
        {
            await LogError(logRequest, nameof(LogEntryAsync));
        }

        public async Task LogInfo(LogRequestDto logRequest)
        {
            await LogError(logRequest, nameof(LogInfo));
        }

        public async Task LogWarning(LogRequestDto logRequest)
        {
            await LogError(logRequest, nameof(LogWarning));
        }

        public async Task LogDebug(LogRequestDto logRequest)
        {
            await LogError(logRequest, nameof(LogDebug));
        }

        public async Task LogError(LogRequestDto logRequest)
        {
            await LogError(logRequest, nameof(LogError));
        }

        public async Task LogException(Exception exception)
        {
            await LogError(exception, nameof(LogException));
        }

        public async Task LogErrorDetails(ATMErrorDto logErrorOption)
        {
            await LogError(logErrorOption, nameof(LogErrorDetails));
        }

        public async Task LogCustomError(CustomErrorExtensionDto logErrorOption)
        {
            var IsErrorLog = true; // Temporary set to true for now as profile option is not yet ready, once profile option is ready then we can use the above code and remove this line
            if (IsErrorLog)
            {
                await LogError(logErrorOption, nameof(LogCustomError));
            }
        }


        private async Task LogError<T>(T exception, string action)
        {
            await baseService.SendAsync(new RequestDto()
            {
                ApiType = ApiUtility.ApiType.POST,
                Data = exception,
                Url = LoggingApiBaseUrl + action
            });
        }
    }
}
