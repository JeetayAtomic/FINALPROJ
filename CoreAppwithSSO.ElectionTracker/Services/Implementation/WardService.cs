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
    public class WardService(IWardRepository wardRepository, IMapper mapper) : IWardService
    {
        public async Task<WardResponse?> GetWardByWardId(int id)
        {
            var response = await wardRepository.GetWardByWardId(id);
            if (response is not null)
            {
                return mapper.Map<WardResponse>(response);
            }
            return null;
        }

        public async Task<List<WardResponse>> GetWardList(int constituencyId = 0)
        {
            var response = await wardRepository.GetWardList(constituencyId);
            return mapper.Map<List<WardResponse>>(response);
        }

        public async Task<WardResponse?> SaveWard(WardRequest request)
        {
            Ward ward = mapper.Map<Ward>(request);
            var response = await wardRepository.SaveWard(ward);
            return response;
        }

        public async Task<WardFilterResponse> SearchWardFilter(SearchFilterRequest filterRequest)
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
            WardFilterResponse searchWardResponse = await wardRepository.SearchWardFilter(filterRequest);
            return searchWardResponse;
        }
    }
}
