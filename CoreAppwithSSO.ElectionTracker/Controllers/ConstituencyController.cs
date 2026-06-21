using CoreAppwithSSO.ElectionTracker.Constants;
using CoreAppwithSSO.ElectionTracker.Extension;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;
using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.ElectionTracker.Controllers
{
    [Route(UrlConstant.ConstituencyRoutePrefix)]
    [ApiController]
    public class ConstituencyController(IConstituencyService constituencyService) : BaseController
    {
        [HttpPost(UrlConstant.ManageConstituency)]
        public async Task<IActionResult> ManageConstituency(ConstituencyRequest request)
        {
            var response = await HandleSaveAsync(request,
                                              constituencyService.SaveConstituency,
                                              this.HttpContext,
                                              Constant.SAVE_CONSTITUENCY_SUCCESS,
                                              Constant.SAVE_CONSTITUENCY_ERROR,
                                              (req, userId) => req.SetAuditFields(req.ConstituencyId, userId));

            return Ok(response);
        }

        [HttpGet(UrlConstant.GetConstituencyById)]
        public async Task<IActionResult> GetConstituencyById(int id)
        {
            var response = await HandleGetAsync(
                () => constituencyService.GetConstituencyById(id),
                Constant.GET_CONSTITUENCY_SUCCESS,
                Constant.GET_CONSTITUENCY_ERROR
            );
            return Ok(response);
        }

        [HttpGet(UrlConstant.GetConstituencyList)]
        public async Task<IActionResult> GetConstituencyList([FromQuery] int stateId = 0)
        {
            var response = await HandleListAsync(
                () => constituencyService.GetConstituencyList(stateId),
                Constant.LIST_CONSTITUENCY_SUCCESS,
                Constant.LIST_CONSTITUENCY_ERROR
            );
            return Ok(response);
        }

        [HttpPost(UrlConstant.ConstituencySearch)]
        public async Task<IActionResult> SearchConstituencyByFilter(SearchFilterRequest filterRequest)
        {
            var response = await HandleSearchPagingAsync<ConstituencyFilterResponse, ConstituencyListResponse>(
                () => constituencyService.SearchConstituencyFilter(filterRequest),
                Constant.SEARCH_CONSTITUENCY_SUCCESS,
                Constant.SEARCH_CONSTITUENCY_ERROR
            );
            return Ok(response);
        }
    }
}
