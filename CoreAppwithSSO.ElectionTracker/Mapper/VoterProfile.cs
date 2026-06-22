using AutoMapper;
using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Mapper
{
    public class VoterProfile : Profile
    {
        public VoterProfile()
        {
            CreateMap<VoterRequest, Voter>();
            CreateMap<Voter, VoterResponse>();
        }
    }
}
