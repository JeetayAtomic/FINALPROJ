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
    public class VoterService(IVoterRepository voterRepository, IMapper mapper) : IVoterService
    {
        public async Task<VoterResponse?> GetVoterByVoterId(long id)
        {
            var response = await voterRepository.GetVoterByVoterId(id);
            if (response is not null)
            {
                return mapper.Map<VoterResponse>(response);
            }
            return null;
        }

        public async Task<List<VoterResponse>> GetVoterList(int boothId = 0)
        {
            var response = await voterRepository.GetVoterList(boothId);
            return mapper.Map<List<VoterResponse>>(response);
        }

        public async Task<VoterResponse?> SaveVoter(VoterRequest request)
        {
            Voter voter = mapper.Map<Voter>(request);
            var response = await voterRepository.SaveVoter(voter);
            return response;
        }

        public async Task<VoterFilterResponse> SearchVoterFilter(SearchFilterRequest filterRequest)
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
            VoterFilterResponse searchVoterResponse = await voterRepository.SearchVoterFilter(filterRequest);
            return searchVoterResponse;
        }
    }
}
