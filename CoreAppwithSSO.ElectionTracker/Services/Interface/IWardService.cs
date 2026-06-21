using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Services.Interface
{
    public interface IWardService
    {
        Task<WardResponse?> SaveWard(WardRequest ward);
        Task<WardResponse?> GetWardByWardId(int id);
        Task<List<WardResponse>> GetWardList(int constituencyId = 0);
        Task<WardFilterResponse> SearchWardFilter(SearchFilterRequest filterRequest);
    }
}
