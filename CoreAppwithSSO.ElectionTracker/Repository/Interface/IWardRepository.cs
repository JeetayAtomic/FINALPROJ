using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Repository.Interface
{
    public interface IWardRepository
    {
        Task<WardResponse?> SaveWard(Ward ward);
        Task<Ward?> GetWardByWardId(int id);
        Task<List<WardResponse>> GetWardList(int constituencyId = 0);
        Task<WardFilterResponse> SearchWardFilter(SearchFilterRequest filterRequest);
    }
}
