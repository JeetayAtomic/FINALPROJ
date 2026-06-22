using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Repository.Interface
{
    public interface IKaryakartaRepository
    {
        Task<KaryakartaResponse?> SaveKaryakarta(Karyakarta karyakarta);
        Task<Karyakarta?> GetKaryakartaByKaryakartaId(int id);
        Task<List<KaryakartaResponse>> GetKaryakartaList(int boothId = 0);
        Task<KaryakartaFilterResponse> SearchKaryakartaFilter(SearchFilterRequest filterRequest);
    }
}
