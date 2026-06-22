using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Repository.Interface
{
    public interface IVoterRepository
    {
        Task<VoterResponse?> SaveVoter(Voter voter);
        Task<Voter?> GetVoterByVoterId(long id);
        Task<List<VoterResponse>> GetVoterList(int boothId = 0);
        Task<VoterFilterResponse> SearchVoterFilter(SearchFilterRequest filterRequest);
    }
}
