using CoreAppwithSSO.ElectionTracker.Constants;
using CoreAppwithSSO.ElectionTracker.Extension;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;
using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.ElectionTracker.Controllers
{
    [Route(UrlConstant.BoothRoutePrefix)]
    [ApiController]
    public class BoothController(IBoothService boothService) : BaseController
    {
        [HttpPost(UrlConstant.ManageBooth)]
        public async Task<IActionResult> ManageBooth(BoothRequest request)
        {
            var response = await HandleSaveAsync(request,
                                              boothService.SaveBooth,
                                              this.HttpContext,
                                              Constant.SAVE_BOOTH_SUCCESS,
                                              Constant.SAVE_BOOTH_ERROR,
                                              (req, userId) => req.SetAuditFields(req.BoothId, userId));

            return Ok(response);
        }

        [HttpGet(UrlConstant.GetBoothByBoothId)]
        public async Task<IActionResult> GetBoothByBoothId(int id)
        {
            var response = await HandleGetAsync(
                () => boothService.GetBoothByBoothId(id),
                Constant.GET_BOOTH_SUCCESS,
                Constant.GET_BOOTH_ERROR
            );
            return Ok(response);
        }

        [HttpGet(UrlConstant.GetBoothList)]
        public async Task<IActionResult> GetBoothList([FromQuery] int wardId = 0)
        {
            var response = await HandleListAsync(
                () => boothService.GetBoothList(wardId),
                Constant.LIST_BOOTH_SUCCESS,
                Constant.LIST_BOOTH_ERROR
            );
            return Ok(response);
        }

        [HttpPost(UrlConstant.BoothSearch)]
        public async Task<IActionResult> SearchBoothByFilter(SearchFilterRequest filterRequest)
        {
            var response = await HandleSearchPagingAsync<BoothFilterResponse, BoothListResponse>(
                () => boothService.SearchBoothFilter(filterRequest),
                Constant.SEARCH_BOOTH_SUCCESS,
                Constant.SEARCH_BOOTH_ERROR
            );
            return Ok(response);
        }
    }
}
