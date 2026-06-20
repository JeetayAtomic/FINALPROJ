using AutoMapper;
using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Mapper
{
    public class BoothProfile : Profile
    {
        public BoothProfile()
        {
            CreateMap<BoothRequest, Booth>();
            CreateMap<Booth, BoothResponse>();
        }
    }
}
