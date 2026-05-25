using CoreAppwithSSO.API.Common;
using CoreAppwithSSO.API.DTOs;
using CoreAppwithSSO.API.DTOs.Client;
using CoreAppwithSSO.API.Helper;
using CoreAppwithSSO.API.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace CoreAppwithSSO.API.Services
{
    public interface IClientService
    {
        Task<(CreateOppFinalResponse, OppMainResponse, List<OppDetailsResponse>, ShipInstructionResponse)> CreateOppAsync(CreateOppRequest request);

        Task<List<ClientResponse>> GetClientsAsync(string? q = null, string? profile = null);

        Task<List<DangerousGoodResponse>> GetDangerousGoodsAsync();

        Task<List<ServiceLevelResponse>> GetServiceLevelAsync();

        Task<List<ShipInstrunctionResponse>> GetShipInstructionsAsync();

        Task<List<TraceInfoResponse>> GetTraceInfoAsync(string billNumber);

        Task<List<SealInfoResponse>> GetSealInfoAsync(string billNumber);

        Task<List<OrderInfoResponse>> GetOrderInfoAsync(string billNumber);
    }
    public class ClientService : IClientService
    {
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;
        public ClientService(HttpClient httpClient, IOptions<AppSettings> appSettings)
        {
            _httpClient = httpClient;
            _appSettings = appSettings.Value;
        }

        public async Task<(CreateOppFinalResponse, OppMainResponse , List<OppDetailsResponse> , ShipInstructionResponse)> CreateOppAsync(CreateOppRequest request)
        {
            var finalResponse = new CreateOppFinalResponse();
            var oppMainResponse = new OppMainResponse();
            var oppDetailsRespone = new  List<OppDetailsResponse>();
            var shipInstructionResponse = new ShipInstructionResponse();
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = new LowerCaseNamingPolicy()
                };

                // 1. MAIN API

                var mainJson = JsonSerializer.Serialize(request.Main, options);
                var mainContent = new StringContent(mainJson, Encoding.UTF8, "application/json");

                var mainHttpRes = await _httpClient.PostAsync(
                    _appSettings.APIURL + ApiEndpoints.UpsertPreOrder,
                    mainContent);

                oppMainResponse = await mainHttpRes.Content
                    .ReadFromJsonAsync<OppMainResponse>();

                if (!mainHttpRes.IsSuccessStatusCode || oppMainResponse?.Result?.HasError == true)
                {
                    finalResponse.Errors.Add("Main API failed");
                }


                // 2. DETAILS API (LOOP PER RECORD)

                var detailsResponses = new List<OppDetailsResponse>();

                foreach (var detail in request.Details)
                {
                    var detailsJson = JsonSerializer.Serialize(detail, options);
                    var detailsContent = new StringContent(detailsJson, Encoding.UTF8, "application/json");

                    var detailsHttpRes = await _httpClient.PostAsync(
                        _appSettings.APIURL + ApiEndpoints.UpsertPreOrderDetails,
                        detailsContent);

                    var details = await detailsHttpRes.Content
                        .ReadFromJsonAsync<OppDetailsResponse>();

                    if (!detailsHttpRes.IsSuccessStatusCode || details?.Result?.HasError == true)
                    {
                        finalResponse.Errors.Add(details?.Result?.ErrorMessage);
                        detailsResponses.Add(details);

                    }
                    else
                    {
                        detailsResponses.Add(details);
                    }
                }

                // Take first successful response for merge (keep your existing structure)
                var firstDetails = detailsResponses.FirstOrDefault();
                oppDetailsRespone = detailsResponses;


                // 3. SHIP INSTRUCTIONS API

                if (request.ShipInstructions.Instruction_Ids.Any())
                {
                    //finalResponse.Errors.Add("Ship Instructions Ids are not coming");
                    var shipJson = JsonSerializer.Serialize(request.ShipInstructions, options);
                    var shipContent = new StringContent(shipJson, Encoding.UTF8, "application/json");

                    var shipHttpRes = await _httpClient.PostAsync(
                        _appSettings.APIURL + ApiEndpoints.UpsertPreOrderShipInstructions,
                        shipContent);

                    shipInstructionResponse = await shipHttpRes.Content
                        .ReadFromJsonAsync<ShipInstructionResponse>();

                    if (!shipHttpRes.IsSuccessStatusCode || shipInstructionResponse?.Result?.HasError == true)
                    {
                        finalResponse.Errors.Add("Ship Instructions API failed");
                    }
                }


                finalResponse.IsSuccess = finalResponse.Errors.Count == 0;

                return (finalResponse, oppMainResponse, oppDetailsRespone, shipInstructionResponse);
            }
            catch (Exception ex)
            {
                finalResponse.IsSuccess = false;
                finalResponse.Errors.Add(ex.Message);

                return (finalResponse, oppMainResponse, oppDetailsRespone, shipInstructionResponse);
            }
        }

        public async Task<List<ClientResponse>> GetClientsAsync(string? q = null, string? profile = null)
        {
            var url = _appSettings.APIURL + ApiEndpoints.Clients;

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(q)) queryParams.Add($"q={Uri.EscapeDataString(q)}");
            if (!string.IsNullOrEmpty(profile)) queryParams.Add($"profile={Uri.EscapeDataString(profile)}");

            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var clients = JsonSerializer.Deserialize<List<ClientResponse>>(json);

            return clients;
        }

        public async Task<List<DangerousGoodResponse>> GetDangerousGoodsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _appSettings.APIURL + ApiEndpoints.DangeroursGood);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var dangerousGoods = JsonSerializer.Deserialize<List<DangerousGoodResponse>>(json);
            return dangerousGoods;
        }

        public async Task<List<ServiceLevelResponse>> GetServiceLevelAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _appSettings.APIURL + ApiEndpoints.Servicelevels);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var serviceLevelResponse = JsonSerializer.Deserialize<List<ServiceLevelResponse>>(json);
            return serviceLevelResponse;
        }

        public async Task<List<ShipInstrunctionResponse>> GetShipInstructionsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _appSettings.APIURL + ApiEndpoints.Shipinstructions);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var serviceLevelResponse = JsonSerializer.Deserialize<List<ShipInstrunctionResponse>>(json);
            return serviceLevelResponse;
        }

        public async Task<List<TraceInfoResponse>> GetTraceInfoAsync(string billNumber)
        {
            var url = $"{_appSettings.APIURL + ApiEndpoints.TraceBill}={Uri.EscapeDataString(billNumber)}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var traceInfoResponse = JsonSerializer.Deserialize<List<TraceInfoResponse>>(json);
            return traceInfoResponse;
        }

        public async Task<List<SealInfoResponse>> GetSealInfoAsync(string billNumber)
        {
            var url = $"{_appSettings.APIURL + ApiEndpoints.SealsBill} = {Uri.EscapeDataString(billNumber)}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var sealInfoResponse = JsonSerializer.Deserialize<List<SealInfoResponse>>(json);
            return sealInfoResponse;
        }

        public async Task<List<OrderInfoResponse>> GetOrderInfoAsync(string billNumber)
        {
            var url = $"{_appSettings.APIURL + ApiEndpoints.OrderBill}={Uri.EscapeDataString(billNumber)}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var orderInfoResponse = JsonSerializer.Deserialize<List<OrderInfoResponse>>(json);
            return orderInfoResponse;
        }
    }
}
