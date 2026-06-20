using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Repository.Interface
{
    public interface IBoothRepository
    {
        Task<BoothResponse?> SaveBooth(Booth booth);
        Task<Booth?> GetBoothByBoothId(int id);
        Task<List<BoothResponse>> GetBoothList(int wardId = 0);
        Task<BoothFilterResponse> SearchBoothFilter(SearchFilterRequest filterRequest);
    }
}
