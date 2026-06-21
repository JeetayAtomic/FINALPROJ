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
    public class ConstituencyRepository(DapperContext dapperContext) : IConstituencyRepository
    {
        // Schema-qualified, UNbracketed: the dialect's QuoteName brackets each dot-separated
        // part, so "ELT.Constituency" becomes [ELT].[Constituency]. Storing it pre-bracketed
        // here would be double-quoted into the invalid [[ELT]].[[Constituency]].
        private const string ConstituencyTable = "ELT.Constituency";
        private const string ConstituencyKey = "ConstituencyId";

        public async Task<Constituency?> GetConstituencyById(int id)
        {
            Constituency? constituency = null;
            var dialect = dapperContext.Dialect;
            string query = $"SELECT * FROM {dialect.QuoteName(ConstituencyTable)} WHERE {dialect.QuoteName(ConstituencyKey)} = @ConstituencyId";
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ConstituencyId", id, DbType.Int32);
                constituency = await connection.QueryFirstOrDefaultAsync<Constituency>(query, parameters, commandType: CommandType.Text);
            }
            return constituency;
        }

        public async Task<List<ConstituencyResponse>> GetConstituencyList(int stateId = 0)
        {
            string query = DBProcedure.GetConstituencyList;
            using var connection = dapperContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("StateId", stateId);
            return (await connection.QueryAsync<ConstituencyResponse>(query, parameters, commandType: CommandType.StoredProcedure))?.ToList() ?? [];
        }

        public async Task<ConstituencyResponse?> SaveConstituency(Constituency constituency)
        {
            return constituency.ConstituencyId > 0
                ? await UpdateConstituency(constituency)
                : await AddConstituency(constituency);
        }

        public async Task<ConstituencyFilterResponse> SearchConstituencyFilter(SearchFilterRequest filterRequest)
        {
            ConstituencyFilterResponse searchResult = new();
            string query = DBProcedure.SearchConstituencyByFilter;
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

            var response = await connection.QueryAsync<ConstituencyListResponse>(query, sqlParams, commandType: CommandType.StoredProcedure);
            if (response != null && response.Any())
            {
                searchResult.Results = [.. response];
                searchResult.TotalRecord = response.FirstOrDefault()?.TotalRecord ?? 0;
            }
            return searchResult;
        }

        private async Task<ConstituencyResponse?> AddConstituency(Constituency constituency)
        {
            ConstituencyResponse? response = null;
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                // ConstituencyId is an identity column, so it is excluded from the INSERT.
                string query = CommonUtl.BuildInsertQuery(parameters, constituency, ConstituencyTable, dapperContext.Dialect, ["ConstituencyId"]);
                response = await connection.QueryFirstOrDefaultAsync<ConstituencyResponse>(query, parameters, commandType: CommandType.Text);
            }
            return response;
        }

        private async Task<ConstituencyResponse?> UpdateConstituency(Constituency constituency)
        {
            ConstituencyResponse? response = null;
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                // ConstituencyId drives the WHERE clause; CreatedBy must not be overwritten on update.
                string query = CommonUtl.BuildUpdateQuery(parameters, constituency, ConstituencyTable, ConstituencyKey, dapperContext.Dialect, ["CreatedBy"]);
                response = await connection.QueryFirstOrDefaultAsync<ConstituencyResponse>(query, parameters, commandType: CommandType.Text);
            }
            return response;
        }
    }
}
