using AutoMapper;
using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Mapper
{
    public class WardProfile : Profile
    {
        public WardProfile()
        {
            CreateMap<WardRequest, Ward>();
            CreateMap<Ward, WardResponse>();
        }
    }
}
