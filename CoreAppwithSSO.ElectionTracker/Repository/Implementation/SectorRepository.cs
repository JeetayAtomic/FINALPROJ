using CoreAppwithSSO.ElectionTracker.Common;
using CoreAppwithSSO.ElectionTracker.Data;
using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Request;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;
using CoreAppwithSSO.ElectionTracker.Repository.Interface;
using Dapper;
using System.Data;

namespace CoreAppwithSSO.ElectionTracker.Repository.Implementation
{
    public class SectorRepository(DapperContext dapperContext) : ISectorRepository
    {
        // Schema-qualified, UNbracketed: the dialect's QuoteName brackets each dot-separated
        // part, so "ELT.Sector" becomes [ELT].[Sector]. Storing it pre-bracketed here would be
        // double-quoted into the invalid [[ELT]].[[Sector]].
        private const string SectorTable = "ELT.Sector";
        private const string SectorKey = "SectorId";

        public async Task<Sector?> GetSectorBySectorId(int id)
        {
            Sector? sector = null;
            var dialect = dapperContext.Dialect;
            string query = $"SELECT * FROM {dialect.QuoteName(SectorTable)} WHERE {dialect.QuoteName(SectorKey)} = @SectorId";
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@SectorId", id, DbType.Int32);
                sector = await connection.QueryFirstOrDefaultAsync<Sector>(query, parameters, commandType: CommandType.Text);
            }
            return sector;
        }

        public async Task<List<SectorResponse>> GetSectorList(int boothId = 0)
        {
            string query = DBProcedure.GetSectorList;
            using var connection = dapperContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("BoothId", boothId);
            return (await connection.QueryAsync<SectorResponse>(query, parameters, commandType: CommandType.StoredProcedure))?.ToList() ?? [];
        }

        public async Task<SectorResponse?> SaveSector(Sector sector)
        {
            return sector.SectorId > 0
                ? await UpdateSector(sector)
                : await AddSector(sector);
        }

        public async Task<SectorFilterResponse> SearchSectorFilter(SearchFilterRequest filterRequest)
        {
            SectorFilterResponse searchResult = new();
            string query = DBProcedure.SearchSectorByFilter;
            string dynamicSearchParameters = string.Empty;
            if ((filterRequest.DynamicSearchParameters?.Count ?? 0) > 0)
            {
                CommonUtl.SetDynamicParametersData(filterRequest.DynamicSearchParameters ?? [], out dynamicSearchParameters);
            }
            var sqlParams = new
            {
                filterRequest.SortColumn,
                filterRequest.SortOrder,
                PageNo = filterRequest.PageIndex,
                filterRequest.PageSize,
                DynamicSearchParameters = dynamicSearchParameters,
            };

            using var connection = dapperContext.CreateConnection();

            var response = await connection.QueryAsync<SectorListResponse>(query, sqlParams, commandType: CommandType.StoredProcedure);
            if (response != null && response.Any())
            {
                searchResult.Results = [.. response];
                searchResult.TotalRecord = response.FirstOrDefault()?.TotalRecord ?? 0;
            }
            return searchResult;
        }

        private async Task<SectorResponse?> AddSector(Sector sector)
        {
            SectorResponse? response = null;
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                // SectorId is an identity column, so it is excluded from the INSERT.
                string query = CommonUtl.BuildInsertQuery(parameters, sector, SectorTable, dapperContext.Dialect, ["SectorId"]);
                response = await connection.QueryFirstOrDefaultAsync<SectorResponse>(query, parameters, commandType: CommandType.Text);
            }
            return response;
        }

        private async Task<SectorResponse?> UpdateSector(Sector sector)
        {
            SectorResponse? response = null;
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                // SectorId drives the WHERE clause; CreatedBy must not be overwritten on update.
                string query = CommonUtl.BuildUpdateQuery(parameters, sector, SectorTable, SectorKey, dapperContext.Dialect, ["CreatedBy"]);
                response = await connection.QueryFirstOrDefaultAsync<SectorResponse>(query, parameters, commandType: CommandType.Text);
            }
            return response;
        }
    }
}
