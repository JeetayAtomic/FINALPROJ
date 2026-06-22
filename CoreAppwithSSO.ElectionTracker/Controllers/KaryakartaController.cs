using CoreAppwithSSO.ElectionTracker.Constants;
using CoreAppwithSSO.ElectionTracker.Extension;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;
using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.ElectionTracker.Controllers
{
    [Route(UrlConstant.KaryakartaRoutePrefix)]
    [ApiController]
    public class KaryakartaController(IKaryakartaService karyakartaService) : BaseController
    {
        [HttpPost(UrlConstant.ManageKaryakarta)]
        public async Task<IActionResult> ManageKaryakarta(KaryakartaRequest request)
        {
            var response = await HandleSaveAsync(request,
                                              karyakartaService.SaveKaryakarta,
                                              this.HttpContext,
                                              Constant.SAVE_KARYAKARTA_SUCCESS,
                                              Constant.SAVE_KARYAKARTA_ERROR,
                                              (req, userId) => req.SetAuditFields(req.KaryakartaId, userId));

            return Ok(response);
        }

        [HttpGet(UrlConstant.GetKaryakartaByKaryakartaId)]
        public async Task<IActionResult> GetKaryakartaByKaryakartaId(int id)
        {
            var response = await HandleGetAsync(
                () => karyakartaService.GetKaryakartaByKaryakartaId(id),
                Constant.GET_KARYAKARTA_SUCCESS,
                Constant.GET_KARYAKARTA_ERROR
            );
            return Ok(response);
        }

        [HttpGet(UrlConstant.GetKaryakartaList)]
        public async Task<IActionResult> GetKaryakartaList([FromQuery] int boothId = 0)
        {
            var response = await HandleListAsync(
                () => karyakartaService.GetKaryakartaList(boothId),
                Constant.LIST_KARYAKARTA_SUCCESS,
                Constant.LIST_KARYAKARTA_ERROR
            );
            return Ok(response);
        }

        [HttpPost(UrlConstant.KaryakartaSearch)]
        public async Task<IActionResult> SearchKaryakartaByFilter(SearchFilterRequest filterRequest)
        {
            var response = await HandleSearchPagingAsync<KaryakartaFilterResponse, KaryakartaListResponse>(
                () => karyakartaService.SearchKaryakartaFilter(filterRequest),
                Constant.SEARCH_KARYAKARTA_SUCCESS,
                Constant.SEARCH_KARYAKARTA_ERROR
            );
            return Ok(response);
        }
    }
}
