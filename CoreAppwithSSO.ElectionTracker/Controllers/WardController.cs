using CoreAppwithSSO.ElectionTracker.Constants;
using CoreAppwithSSO.ElectionTracker.Extension;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;
using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.ElectionTracker.Controllers
{
    [Route(UrlConstant.WardRoutePrefix)]
    [ApiController]
    public class WardController(IWardService wardService) : BaseController
    {
        [HttpPost(UrlConstant.ManageWard)]
        public async Task<IActionResult> ManageWard(WardRequest request)
        {
            var response = await HandleSaveAsync(request,
                                              wardService.SaveWard,
                                              this.HttpContext,
                                              Constant.SAVE_WARD_SUCCESS,
                                              Constant.SAVE_WARD_ERROR,
                                              (req, userId) => req.SetAuditFields(req.WardId, userId));

            return Ok(response);
        }

        [HttpGet(UrlConstant.GetWardByWardId)]
        public async Task<IActionResult> GetWardByWardId(int id)
        {
            var response = await HandleGetAsync(
                () => wardService.GetWardByWardId(id),
                Constant.GET_WARD_SUCCESS,
                Constant.GET_WARD_ERROR
            );
            return Ok(response);
        }

        [HttpGet(UrlConstant.GetWardList)]
        public async Task<IActionResult> GetWardList([FromQuery] int constituencyId = 0)
        {
            var response = await HandleListAsync(
                () => wardService.GetWardList(constituencyId),
                Constant.LIST_WARD_SUCCESS,
                Constant.LIST_WARD_ERROR
            );
            return Ok(response);
        }

        [HttpPost(UrlConstant.WardSearch)]
        public async Task<IActionResult> SearchWardByFilter(SearchFilterRequest filterRequest)
        {
            var response = await HandleSearchPagingAsync<WardFilterResponse, WardListResponse>(
                () => wardService.SearchWardFilter(filterRequest),
                Constant.SEARCH_WARD_SUCCESS,
                Constant.SEARCH_WARD_ERROR
            );
            return Ok(response);
        }
    }
}
