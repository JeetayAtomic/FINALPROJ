using AutoMapper;
using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Mapper
{
    public class KaryakartaProfile : Profile
    {
        public KaryakartaProfile()
        {
            CreateMap<KaryakartaRequest, Karyakarta>();
            CreateMap<Karyakarta, KaryakartaResponse>();
        }
    }
}
