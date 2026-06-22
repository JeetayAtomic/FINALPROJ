using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;

namespace CoreAppwithSSO.ElectionTracker.Services.Interface
{
    public interface IKaryakartaService
    {
        Task<KaryakartaResponse?> SaveKaryakarta(KaryakartaRequest karyakarta);
        Task<KaryakartaResponse?> GetKaryakartaByKaryakartaId(int id);
        Task<List<KaryakartaResponse>> GetKaryakartaList(int boothId = 0);
        Task<KaryakartaFilterResponse> SearchKaryakartaFilter(SearchFilterRequest filterRequest);
    }
}
