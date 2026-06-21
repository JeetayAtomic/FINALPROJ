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
    public class WardRepository(DapperContext dapperContext) : IWardRepository
    {
        // Schema-qualified, UNbracketed: the dialect's QuoteName brackets each dot-separated
        // part, so "ELT.Ward" becomes [ELT].[Ward]. Storing it pre-bracketed here would be
        // double-quoted into the invalid [[ELT]].[[Ward]].
        private const string WardTable = "ELT.Ward";
        private const string WardKey = "WardId";

        public async Task<Ward?> GetWardByWardId(int id)
        {
            Ward? ward = null;
            var dialect = dapperContext.Dialect;
            string query = $"SELECT * FROM {dialect.QuoteName(WardTable)} WHERE {dialect.QuoteName(WardKey)} = @WardId";
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@WardId", id, DbType.Int32);
                ward = await connection.QueryFirstOrDefaultAsync<Ward>(query, parameters, commandType: CommandType.Text);
            }
            return ward;
        }

        public async Task<List<WardResponse>> GetWardList(int constituencyId = 0)
        {
            string query = DBProcedure.GetWardList;
            using var connection = dapperContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("ConstituencyId", constituencyId);
            return (await connection.QueryAsync<WardResponse>(query, parameters, commandType: CommandType.StoredProcedure))?.ToList() ?? [];
        }

        public async Task<WardResponse?> SaveWard(Ward ward)
        {
            return ward.WardId > 0
                ? await UpdateWard(ward)
                : await AddWard(ward);
        }

        public async Task<WardFilterResponse> SearchWardFilter(SearchFilterRequest filterRequest)
        {
            WardFilterResponse searchResult = new();
            string query = DBProcedure.SearchWardByFilter;
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

            var response = await connection.QueryAsync<WardListResponse>(query, sqlParams, commandType: CommandType.StoredProcedure);
            if (response != null && response.Any())
            {
                searchResult.Results = [.. response];
                searchResult.TotalRecord = response.FirstOrDefault()?.TotalRecord ?? 0;
            }
            return searchResult;
        }

        private async Task<WardResponse?> AddWard(Ward ward)
        {
            WardResponse? response = null;
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                // WardId is an identity column, so it is excluded from the INSERT.
                string query = CommonUtl.BuildInsertQuery(parameters, ward, WardTable, dapperContext.Dialect, ["WardId"]);
                response = await connection.QueryFirstOrDefaultAsync<WardResponse>(query, parameters, commandType: CommandType.Text);
            }
            return response;
        }

        private async Task<WardResponse?> UpdateWard(Ward ward)
        {
            WardResponse? response = null;
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                // WardId drives the WHERE clause; CreatedBy must not be overwritten on update.
                string query = CommonUtl.BuildUpdateQuery(parameters, ward, WardTable, WardKey, dapperContext.Dialect, ["CreatedBy"]);
                response = await connection.QueryFirstOrDefaultAsync<WardResponse>(query, parameters, commandType: CommandType.Text);
            }
            return response;
        }
    }
}
