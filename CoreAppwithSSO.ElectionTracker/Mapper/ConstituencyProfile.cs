using AutoMapper;
using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Mapper
{
    public class ConstituencyProfile : Profile
    {
        public ConstituencyProfile()
        {
            CreateMap<ConstituencyRequest, Constituency>();
            CreateMap<Constituency, ConstituencyResponse>();
        }
    }
}
