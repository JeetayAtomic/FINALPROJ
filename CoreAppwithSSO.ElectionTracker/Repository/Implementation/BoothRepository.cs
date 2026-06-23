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
    public class BoothRepository(DapperContext dapperContext, IEntityCodeRepository entityCodeRepository) : IBoothRepository
    {
        // Schema-qualified, UNbracketed: the dialect's QuoteName brackets each dot-separated
        // part, so "ELT.Booth" becomes [ELT].[Booth]. Storing it pre-bracketed here would be
        // double-quoted into the invalid [[ELT]].[[Booth]].
        private const string BoothTable = "ELT.Booth";
        private const string BoothKey = "BoothId";
        public async Task<Booth?> GetBoothByBoothId(int id)
        {
            Booth? booth = null;
            var dialect = dapperContext.Dialect;
            string query = $"SELECT * FROM {dialect.QuoteName(BoothTable)} WHERE {dialect.QuoteName(BoothKey)} = @BoothId";
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@BoothId", id, DbType.Int32);
                booth = await connection.QueryFirstOrDefaultAsync<Booth>(query, parameters, commandType: CommandType.Text);
            }
            return booth;
        }

        public async Task<List<BoothResponse>> GetBoothList(int wardId = 0)
        {
            string query = DBProcedure.GetBoothList;
            using var connection = dapperContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("WardId", wardId);
            return (await connection.QueryAsync<BoothResponse>(query, parameters, commandType: CommandType.StoredProcedure))?.ToList() ?? [];
        }

        public async Task<BoothResponse?> SaveBooth(Booth booth)
        {
            return booth.BoothId > 0
                ? await UpdateBooth(booth)
                : await AddBooth(booth);
        }

        public async Task<BoothFilterResponse> SearchBoothFilter(SearchFilterRequest filterRequest)
        {
            BoothFilterResponse searchResult = new();
            string query = DBProcedure.SearchBoothByFilter;
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

            var response = await connection.QueryAsync<BoothListResponse>(query, sqlParams, commandType: CommandType.StoredProcedure);
            if (response != null && response.Any())
            {
                searchResult.Results = [.. response];
                searchResult.TotalRecord = response.FirstOrDefault()?.TotalRecord ?? 0;
            }
            return searchResult;
        }

        private async Task<BoothResponse?> AddBooth(Booth booth)
        {
            // BoothCode generation and the INSERT share one transaction so a failed insert
            // rolls back the consumed code sequence (no skipped numbers).
            using var connection = dapperContext.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                booth.BoothCode = await entityCodeRepository.GenerateAsync(
                    nameof(EntityType.Booth), connection, transaction);

                var parameters = new DynamicParameters();
                // BoothId is an identity column, so it is excluded from the INSERT.
                string query = CommonUtl.BuildInsertQuery(parameters, booth, BoothTable, dapperContext.Dialect, ["BoothId"]);
                var response = await connection.QueryFirstOrDefaultAsync<BoothResponse>(
                    new CommandDefinition(query, parameters, transaction, commandType: CommandType.Text));

                transaction.Commit();
                return response;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private async Task<BoothResponse?> UpdateBooth(Booth booth)
        {
            BoothResponse? response = null;
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                // BoothId drives the WHERE clause; CreatedBy must not be overwritten on update.
                string query = CommonUtl.BuildUpdateQuery(parameters, booth, BoothTable, BoothKey, dapperContext.Dialect, ["CreatedBy"]);
                response = await connection.QueryFirstOrDefaultAsync<BoothResponse>(query, parameters, commandType: CommandType.Text);
            }
            return response;
        }
    }
}
