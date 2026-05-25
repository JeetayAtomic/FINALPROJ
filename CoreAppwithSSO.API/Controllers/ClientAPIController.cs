using CoreAppwithSSO.API.Common;
using CoreAppwithSSO.API.Common.Constant;
using CoreAppwithSSO.API.DTOs.Client;
using CoreAppwithSSO.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.API.Controllers
{
    [Route("api/ClientAPI")]
    [ApiController]
    public class ClientAPIController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientAPIController(IClientService clientService)
        {
            _clientService = clientService;
        }
        /// <summary>
        /// Get Clients from MO API
        /// </summary>
        /// <returns>Client List</returns>
        [HttpGet("GetClients")]
        public async Task<IActionResult> GetClients([FromQuery] string? q = null, [FromQuery] string? profile = null)
        {
            APIResponse<List<ClientResponse>> apiResponse = new APIResponse<List<ClientResponse>>();
            try
            {
                var result = await _clientService.GetClientsAsync(q, profile);
                apiResponse.ResponseCode = CommonConstants.GET_CLIENT_SUCCESS;
                apiResponse.Data.Add(result);
            }
            catch (Exception ex)
            {
                apiResponse.IsError = true;
                apiResponse.ResponseCode = CommonConstants.GET_ALLINPUT_ERROR;
                apiResponse.Exceptions.Add(ex.Message);
            }
            return Ok(apiResponse);

        }

        /// <summary>
        /// Get Dangerous Goods from MO API
        /// </summary>
        /// <returns>Client List</returns>
        [HttpGet("GetDangerousGoods")]
        public async Task<IActionResult> GetDangerousGoods()
        {
            APIResponse<List<DangerousGoodResponse>> apiResponse = new APIResponse<List<DangerousGoodResponse>>();
            try
            {
                var result = await _clientService.GetDangerousGoodsAsync();
                apiResponse.ResponseCode = CommonConstants.GET_DANGEROUS_GOODS;
                apiResponse.Data.Add(result);
            }
            catch (Exception ex)
            {
                apiResponse.IsError = true;
                apiResponse.ResponseCode = CommonConstants.GET_ALLINPUT_ERROR;
                apiResponse.Exceptions.Add(ex.Message);
            }
            return Ok(apiResponse);
        }

        /// <summary>
        /// Get Service Levels from MO API
        /// </summary>
        /// <returns>Service Level List</returns>
        [HttpGet("GetServiceLevels")]
        public async Task<IActionResult> GetServiceLevels()
        {
            APIResponse<List<ServiceLevelResponse>> apiResponse = new APIResponse<List<ServiceLevelResponse>>();
            try
            {
                var result = await _clientService.GetServiceLevelAsync();
                apiResponse.ResponseCode = CommonConstants.GET_SERVICE_LEVELS;
                apiResponse.Data.Add(result);
            }
            catch (Exception ex)
            {
                apiResponse.IsError = true;
                apiResponse.ResponseCode = CommonConstants.GET_ALLINPUT_ERROR;
                apiResponse.Exceptions.Add(ex.Message);
            }
            return Ok(apiResponse);
        }

        /// <summary>
        /// Get Ship Instruction from MO API
        /// </summary>
        /// <returns>Ship Instrunction List</returns>
        [HttpGet("GetShipInstructions")]
        public async Task<IActionResult> GetShipInstructions()
        {
            APIResponse<List<ShipInstrunctionResponse>> apiResponse = new APIResponse<List<ShipInstrunctionResponse>>();
            try
            {
                var result = await _clientService.GetShipInstructionsAsync();
                apiResponse.ResponseCode = CommonConstants.GET_SHIP_INSTRUCTIONS;
                apiResponse.Data.Add(result);
            }
            catch (Exception ex)
            {
                apiResponse.IsError = true;
                apiResponse.ResponseCode = CommonConstants.GET_ALLINPUT_ERROR;
                apiResponse.Exceptions.Add(ex.Message);
            }
            return Ok(apiResponse);
        }

        /// <summary>
        /// Get Trace Info from MO API
        /// </summary>
        /// <returns>Trace Information List</returns>
        [HttpGet("GetTraceInfo")]
        public async Task<IActionResult> GetTraceInfo([FromQuery] string billNumber)
        {
            APIResponse<List<TraceInfoResponse>> apiResponse = new APIResponse<List<TraceInfoResponse>>();
            try
            {
                var result = await _clientService.GetTraceInfoAsync(billNumber);
                apiResponse.ResponseCode = CommonConstants.GET_TRACE_INFO;
                apiResponse.Data.Add(result);
            }
            catch (Exception ex)
            {
                apiResponse.IsError = true;
                apiResponse.ResponseCode = CommonConstants.GET_ALLINPUT_ERROR;
                apiResponse.Exceptions.Add(ex.Message);
            }
            return Ok(apiResponse);
        }

        /// <summary>
        /// Get Seals Info from MO API
        /// </summary>
        /// <returns>Seal Information List</returns>
        [HttpGet("GetSealInfo")]
        public async Task<IActionResult> GetSealInfo([FromQuery] string billNumber)
        {
            APIResponse<List<SealInfoResponse>> apiResponse = new APIResponse<List<SealInfoResponse>>();
            try
            {
                var result = await _clientService.GetSealInfoAsync(billNumber);
                apiResponse.ResponseCode = CommonConstants.GET_SEAL_INFO;
                apiResponse.Data.Add(result);
            }
            catch (Exception ex)
            {
                apiResponse.IsError = true;
                apiResponse.ResponseCode = CommonConstants.GET_ALLINPUT_ERROR;
                apiResponse.Exceptions.Add(ex.Message);
            }
            return Ok(apiResponse);
        }

        /// <summary>
        /// Get Order Info from MO API
        /// </summary>
        /// <returns>Order Information List</returns>
        [HttpGet("GetOrderInfo")]
        public async Task<IActionResult> GetOrderInfo([FromQuery] string billNumber)
        {
            APIResponse<List<OrderInfoResponse>> apiResponse = new APIResponse<List<OrderInfoResponse>>();
            try
            {
                var result = await _clientService.GetOrderInfoAsync(billNumber);
                apiResponse.ResponseCode = CommonConstants.GET_ORDER_INFO;
                apiResponse.Data.Add(result);
            }
            catch (Exception ex)
            {
                apiResponse.IsError = true;
                apiResponse.ResponseCode = CommonConstants.GET_ALLINPUT_ERROR;
                apiResponse.Exceptions.Add(ex.Message);
            }
            return Ok(apiResponse);
        }
    }
}
