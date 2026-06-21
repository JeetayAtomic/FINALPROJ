using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Repository.Interface
{
    public interface IConstituencyRepository
    {
        Task<ConstituencyResponse?> SaveConstituency(Constituency constituency);
        Task<Constituency?> GetConstituencyById(int id);
        Task<List<ConstituencyResponse>> GetConstituencyList(int stateId = 0);
        Task<ConstituencyFilterResponse> SearchConstituencyFilter(SearchFilterRequest filterRequest);
    }
}
