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
    public class BoothService(IBoothRepository boothRepository, IMapper mapper) : IBoothService
    {
        public async Task<BoothResponse?> GetBoothByBoothId(int id)
        {
            var response = await boothRepository.GetBoothByBoothId(id);
            if (response is not null)
            {
                return mapper.Map<BoothResponse>(response);
            }
            return null;
        }

        public async Task<List<BoothResponse>> GetBoothList(int wardId = 0)
        {
            var response = await boothRepository.GetBoothList(wardId);
            return mapper.Map<List<BoothResponse>>(response);
        }

        public async Task<BoothResponse?> SaveBooth(BoothRequest request)
        {
            Booth booth = mapper.Map<Booth>(request);
            var response = await boothRepository.SaveBooth(booth);
            return response;
        }

        public async Task<BoothFilterResponse> SearchBoothFilter(SearchFilterRequest filterRequest)
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
            BoothFilterResponse searchBoothResponse = await boothRepository.SearchBoothFilter(filterRequest);
            return searchBoothResponse;
        }
    }
}
