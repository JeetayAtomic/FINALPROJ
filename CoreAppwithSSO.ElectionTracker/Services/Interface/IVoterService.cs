using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Services.Interface
{
    public interface IVoterService
    {
        Task<VoterResponse?> SaveVoter(VoterRequest voter);
        Task<VoterResponse?> GetVoterByVoterId(long id);
        Task<List<VoterResponse>> GetVoterList(int boothId = 0);
        Task<VoterFilterResponse> SearchVoterFilter(SearchFilterRequest filterRequest);
    }
}
