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
    public class VoterRepository(DapperContext dapperContext) : IVoterRepository
    {
        // Schema-qualified, UNbracketed: the dialect's QuoteName brackets each dot-separated
        // part, so "ELT.Voter" becomes [ELT].[Voter]. Storing it pre-bracketed here would be
        // double-quoted into the invalid [[ELT]].[[Voter]].
        private const string VoterTable = "ELT.Voter";
        private const string VoterKey = "VoterId";

        public async Task<Voter?> GetVoterByVoterId(long id)
        {
            Voter? voter = null;
            var dialect = dapperContext.Dialect;
            string query = $"SELECT * FROM {dialect.QuoteName(VoterTable)} WHERE {dialect.QuoteName(VoterKey)} = @VoterId";
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@VoterId", id, DbType.Int64);
                voter = await connection.QueryFirstOrDefaultAsync<Voter>(query, parameters, commandType: CommandType.Text);
            }
            return voter;
        }

        public async Task<List<VoterResponse>> GetVoterList(int boothId = 0)
        {
            string query = DBProcedure.GetVoterList;
            using var connection = dapperContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("BoothId", boothId);
            return (await connection.QueryAsync<VoterResponse>(query, parameters, commandType: CommandType.StoredProcedure))?.ToList() ?? [];
        }

        public async Task<VoterResponse?> SaveVoter(Voter voter)
        {
            return voter.VoterId > 0
                ? await UpdateVoter(voter)
                : await AddVoter(voter);
        }

        public async Task<VoterFilterResponse> SearchVoterFilter(SearchFilterRequest filterRequest)
        {
            VoterFilterResponse searchResult = new();
            string query = DBProcedure.SearchVoterByFilter;
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

            var response = await connection.QueryAsync<VoterListResponse>(query, sqlParams, commandType: CommandType.StoredProcedure);
            if (response != null && response.Any())
            {
                searchResult.Results = [.. response];
                searchResult.TotalRecord = response.FirstOrDefault()?.TotalRecord ?? 0;
            }
            return searchResult;
        }

        private async Task<VoterResponse?> AddVoter(Voter voter)
        {
            VoterResponse? response = null;
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                // VoterId is an identity column, so it is excluded from the INSERT.
                string query = CommonUtl.BuildInsertQuery(parameters, voter, VoterTable, dapperContext.Dialect, ["VoterId"]);
                response = await connection.QueryFirstOrDefaultAsync<VoterResponse>(query, parameters, commandType: CommandType.Text);
            }
            return response;
        }

        private async Task<VoterResponse?> UpdateVoter(Voter voter)
        {
            VoterResponse? response = null;
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                // VoterId drives the WHERE clause; CreatedBy must not be overwritten on update.
                string query = CommonUtl.BuildUpdateQuery(parameters, voter, VoterTable, VoterKey, dapperContext.Dialect, ["CreatedBy"]);
                response = await connection.QueryFirstOrDefaultAsync<VoterResponse>(query, parameters, commandType: CommandType.Text);
            }
            return response;
        }
    }
}
