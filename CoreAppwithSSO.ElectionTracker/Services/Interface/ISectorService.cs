using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Services.Interface
{
    public interface ISectorService
    {
        Task<SectorResponse?> SaveSector(SectorRequest sector);
        Task<SectorResponse?> GetSectorBySectorId(int id);
        Task<List<SectorResponse>> GetSectorList(int boothId = 0);
        Task<SectorFilterResponse> SearchSectorFilter(SearchFilterRequest filterRequest);
    }
}
