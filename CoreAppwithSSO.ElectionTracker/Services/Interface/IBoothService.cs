using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Services.Interface
{
    public interface IBoothService
    {
        Task<BoothResponse?> SaveBooth(BoothRequest booth);
        Task<BoothResponse?> GetBoothByBoothId(int id);
        Task<List<BoothResponse>> GetBoothList(int terminalId = 0);
        Task<BoothFilterResponse> SearchBoothFilter(SearchFilterRequest filterRequest);
    }
}
