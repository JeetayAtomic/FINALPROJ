using CoreAppwithSSO.ElectionTracker.Constants;
using CoreAppwithSSO.ElectionTracker.Extension;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;
using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.ElectionTracker.Controllers
{
    [Route(UrlConstant.VoterRoutePrefix)]
    [ApiController]
    public class VoterController(IVoterService voterService) : BaseController
    {
        [HttpPost(UrlConstant.ManageVoter)]
        public async Task<IActionResult> ManageVoter(VoterRequest request)
        {
            var response = await HandleSaveAsync(request,
                                              voterService.SaveVoter,
                                              this.HttpContext,
                                              Constant.SAVE_VOTER_SUCCESS,
                                              Constant.SAVE_VOTER_ERROR,
                                              (req, userId) => req.SetAuditFields(req.VoterId > 0 ? 1 : 0, userId));

            return Ok(response);
        }

        [HttpGet(UrlConstant.GetVoterByVoterId)]
        public async Task<IActionResult> GetVoterByVoterId(long id)
        {
            var response = await HandleGetAsync(
                () => voterService.GetVoterByVoterId(id),
                Constant.GET_VOTER_SUCCESS,
                Constant.GET_VOTER_ERROR
            );
            return Ok(response);
        }

        [HttpGet(UrlConstant.GetVoterList)]
        public async Task<IActionResult> GetVoterList([FromQuery] int boothId = 0)
        {
            var response = await HandleListAsync(
                () => voterService.GetVoterList(boothId),
                Constant.LIST_VOTER_SUCCESS,
                Constant.LIST_VOTER_ERROR
            );
            return Ok(response);
        }

        [HttpPost(UrlConstant.VoterSearch)]
        public async Task<IActionResult> SearchVoterByFilter(SearchFilterRequest filterRequest)
        {
            var response = await HandleSearchPagingAsync<VoterFilterResponse, VoterListResponse>(
                () => voterService.SearchVoterFilter(filterRequest),
                Constant.SEARCH_VOTER_SUCCESS,
                Constant.SEARCH_VOTER_ERROR
            );
            return Ok(response);
        }
    }
}
