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
    public class SectorService(ISectorRepository sectorRepository, IMapper mapper) : ISectorService
    {
        public async Task<SectorResponse?> GetSectorBySectorId(int id)
        {
            var response = await sectorRepository.GetSectorBySectorId(id);
            if (response is not null)
            {
                return mapper.Map<SectorResponse>(response);
            }
            return null;
        }

        public async Task<List<SectorResponse>> GetSectorList(int boothId = 0)
        {
            var response = await sectorRepository.GetSectorList(boothId);
            return mapper.Map<List<SectorResponse>>(response);
        }

        public async Task<SectorResponse?> SaveSector(SectorRequest request)
        {
            Sector sector = mapper.Map<Sector>(request);
            var response = await sectorRepository.SaveSector(sector);
            return response;
        }

        public async Task<SectorFilterResponse> SearchSectorFilter(SearchFilterRequest filterRequest)
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
            SectorFilterResponse searchSectorResponse = await sectorRepository.SearchSectorFilter(filterRequest);
            return searchSectorResponse;
        }
    }
}
