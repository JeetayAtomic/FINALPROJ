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
    public class ConstituencyService(IConstituencyRepository constituencyRepository, IMapper mapper) : IConstituencyService
    {
        public async Task<ConstituencyResponse?> GetConstituencyById(int id)
        {
            var response = await constituencyRepository.GetConstituencyById(id);
            if (response is not null)
            {
                return mapper.Map<ConstituencyResponse>(response);
            }
            return null;
        }

        public async Task<List<ConstituencyResponse>> GetConstituencyList(int stateId = 0)
        {
            var response = await constituencyRepository.GetConstituencyList(stateId);
            return mapper.Map<List<ConstituencyResponse>>(response);
        }

        public async Task<ConstituencyResponse?> SaveConstituency(ConstituencyRequest request)
        {
            Constituency constituency = mapper.Map<Constituency>(request);
            var response = await constituencyRepository.SaveConstituency(constituency);
            return response;
        }

        public async Task<ConstituencyFilterResponse> SearchConstituencyFilter(SearchFilterRequest filterRequest)
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
            ConstituencyFilterResponse searchConstituencyResponse = await constituencyRepository.SearchConstituencyFilter(filterRequest);
            return searchConstituencyResponse;
        }
    }
}
