using CoreAppwithSSO.ElectionTracker.Constants;
using CoreAppwithSSO.ElectionTracker.Extension;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;
using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.ElectionTracker.Controllers
{
    [Route(UrlConstant.SectorRoutePrefix)]
    [ApiController]
    public class SectorController(ISectorService sectorService) : BaseController
    {
        [HttpPost(UrlConstant.ManageSector)]
        public async Task<IActionResult> ManageSector(SectorRequest request)
        {
            var response = await HandleSaveAsync(request,
                                              sectorService.SaveSector,
                                              this.HttpContext,
                                              Constant.SAVE_SECTOR_SUCCESS,
                                              Constant.SAVE_SECTOR_ERROR,
                                              (req, userId) => req.SetAuditFields(req.SectorId, userId));

            return Ok(response);
        }

        [HttpGet(UrlConstant.GetSectorBySectorId)]
        public async Task<IActionResult> GetSectorBySectorId(int id)
        {
            var response = await HandleGetAsync(
                () => sectorService.GetSectorBySectorId(id),
                Constant.GET_SECTOR_SUCCESS,
                Constant.GET_SECTOR_ERROR
            );
            return Ok(response);
        }

        [HttpGet(UrlConstant.GetSectorList)]
        public async Task<IActionResult> GetSectorList([FromQuery] int boothId = 0)
        {
            var response = await HandleListAsync(
                () => sectorService.GetSectorList(boothId),
                Constant.LIST_SECTOR_SUCCESS,
                Constant.LIST_SECTOR_ERROR
            );
            return Ok(response);
        }

        [HttpPost(UrlConstant.SectorSearch)]
        public async Task<IActionResult> SearchSectorByFilter(SearchFilterRequest filterRequest)
        {
            var response = await HandleSearchPagingAsync<SectorFilterResponse, SectorListResponse>(
                () => sectorService.SearchSectorFilter(filterRequest),
                Constant.SEARCH_SECTOR_SUCCESS,
                Constant.SEARCH_SECTOR_ERROR
            );
            return Ok(response);
        }
    }
}
