using AutoMapper;
using CoreAppwithSSO.ElectionTracker.Common;
using CoreAppwithSSO.ElectionTracker.Models;
using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;
using CoreAppwithSSO.ElectionTracker.Repository.Interface;
using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Newtonsoft.Json.Linq;

namespace CoreAppwithSSO.ElectionTracker.Services.Implementation
{
    public class KaryakartaService(IKaryakartaRepository karyakartaRepository, IMapper mapper) : IKaryakartaService
    {
        public async Task<KaryakartaResponse?> GetKaryakartaByKaryakartaId(int id)
        {
            var response = await karyakartaRepository.GetKaryakartaByKaryakartaId(id);
            if (response is not null)
            {
                return mapper.Map<KaryakartaResponse>(response);
            }
            return null;
        }

        public async Task<List<KaryakartaResponse>> GetKaryakartaList(int boothId = 0)
        {
            var response = await karyakartaRepository.GetKaryakartaList(boothId);
            return mapper.Map<List<KaryakartaResponse>>(response);
        }

        public async Task<KaryakartaResponse?> SaveKaryakarta(KaryakartaRequest request)
        {
            Karyakarta karyakarta = mapper.Map<Karyakarta>(request);
            var response = await karyakartaRepository.SaveKaryakarta(karyakarta);
            return response;
        }

        public async Task<KaryakartaFilterResponse> SearchKaryakartaFilter(SearchFilterRequest filterRequest)
        {
            string jsonString = filterRequest.Filters;
            if (!string.IsNullOrWhiteSpace(jsonString) && jsonString != "[]")
            {
                JArray conditionArray = JArray.Parse(jsonString);
                List<DynamicSearchParameters> searchParameters = [];
                CommonUtl.ExtractConditions(conditionArray, searchParameters);

                filterRequest.DynamicSearchParameters ??= [];
                filterRequest.DynamicSearchParameters.AddRange(searchParameters);
            }
            KaryakartaFilterResponse searchKaryakartaResponse = await karyakartaRepository.SearchKaryakartaFilter(filterRequest);
            return searchKaryakartaResponse;
        }
    }
}
