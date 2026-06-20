using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;
using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static CoreAppwithSSO.ElectionTracker.Models.DTO.ApiUtility;

namespace CoreAppwithSSO.ElectionTracker.Services.Implementation
{
    public class BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider) : IBaseService
    {
        public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
        {
            try
            {
                HttpClient client = httpClientFactory.CreateClient("ETSolutionAPI");
                HttpRequestMessage message = new();
                if (requestDto.ContentType == ContentType.MultipartFormData)
                {
                    message.Headers.Add("Accept", "*/*");
                }
                else
                {
                    message.Headers.Add("Accept", "application/json");
                }

                //token
                if (withBearer)
                {
                    var token = tokenProvider.GetToken();
                    message.Headers.Add("Authorization", $"Bearer {token}");
                }

                message.RequestUri = new Uri(requestDto.Url);

                if (requestDto.ContentType == ContentType.MultipartFormData)
                {
                    var content = new MultipartFormDataContent();

                    foreach (var prop in requestDto.Data.GetType().GetProperties())
                    {
                        var value = prop.GetValue(requestDto.Data);
                        if (value is FormFile)
                        {
                            var file = (FormFile)value;
                            if (file != null)
                            {
                                content.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                            }
                        }
                        else
                        {
                            //content.Add(new StringContent(value?.ToString() ?? ""), prop.Name);
                            content.Add(new StringContent(value == null ? "" : value.ToString()), prop.Name);
                        }
                    }
                    message.Content = content;
                }
                else
                {
                    if (requestDto.Data != null)
                    {
                        message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
                    }
                }

                HttpResponseMessage? apiResponse = null;

                switch (requestDto.ApiType)
                {
                    case ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    case ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }

                apiResponse = await client.SendAsync(message);

                switch (apiResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new() { IsError = true, ResponseCode = "Not Found" };
                    case HttpStatusCode.Forbidden:
                        return new() { IsError = true, ResponseCode = "Access Denied" };
                    case HttpStatusCode.Unauthorized:
                        return new() { IsError = true, ResponseCode = "Unauthorized" };
                    case HttpStatusCode.InternalServerError:
                        var apiContent = await apiResponse.Content.ReadAsStringAsync();
                        return new() { IsError = true, ResponseCode = "Internal Server Error" };
                    case HttpStatusCode.BadRequest:
                        var apiContentBad = await apiResponse.Content.ReadAsStringAsync();
                        return new() { IsError = true, ResponseCode = apiContentBad };
                    default:
                        var apiContentCorrect = await apiResponse.Content.ReadAsStringAsync();
                        var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContentCorrect);
                        return apiResponseDto;
                }
            }
            catch (Exception ex)
            {
                var dto = new ResponseDto
                {
                    ResponseCode = ex.Message.ToString(),
                    IsError = true
                };
                return dto;
            }
        }
    }
}
