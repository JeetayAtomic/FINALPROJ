using AutoMapper;
using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Mapper
{
    public class SectorProfile : Profile
    {
        public SectorProfile()
        {
            CreateMap<SectorRequest, Sector>();
            CreateMap<Sector, SectorResponse>();
        }
    }
}
