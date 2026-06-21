using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Repository.Interface
{
    public interface ISectorRepository
    {
        Task<SectorResponse?> SaveSector(Sector sector);
        Task<Sector?> GetSectorBySectorId(int id);
        Task<List<SectorResponse>> GetSectorList(int boothId = 0);
        Task<SectorFilterResponse> SearchSectorFilter(SearchFilterRequest filterRequest);
    }
}
