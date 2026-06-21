using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Services.Interface
{
    public interface IConstituencyService
    {
        Task<ConstituencyResponse?> SaveConstituency(ConstituencyRequest constituency);
        Task<ConstituencyResponse?> GetConstituencyById(int id);
        Task<List<ConstituencyResponse>> GetConstituencyList(int stateId = 0);
        Task<ConstituencyFilterResponse> SearchConstituencyFilter(SearchFilterRequest filterRequest);
    }
}
