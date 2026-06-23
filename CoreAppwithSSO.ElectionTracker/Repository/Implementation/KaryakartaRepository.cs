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
    public class KaryakartaRepository(DapperContext dapperContext, IEntityCodeRepository entityCodeRepository) : IKaryakartaRepository
    {
        // Schema-qualified, UNbracketed: the dialect's QuoteName brackets each dot-separated
        // part, so "ELT.Karyakarta" becomes [ELT].[Karyakarta]. Storing it pre-bracketed here
        // would be double-quoted into the invalid [[ELT]].[[Karyakarta]].
        private const string KaryakartaTable = "ELT.Karyakarta";
        private const string KaryakartaKey = "KaryakartaId";

        public async Task<Karyakarta?> GetKaryakartaByKaryakartaId(int id)
        {
            Karyakarta? karyakarta = null;
            var dialect = dapperContext.Dialect;
            string query = $"SELECT * FROM {dialect.QuoteName(KaryakartaTable)} WHERE {dialect.QuoteName(KaryakartaKey)} = @KaryakartaId";
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@KaryakartaId", id, DbType.Int32);
                karyakarta = await connection.QueryFirstOrDefaultAsync<Karyakarta>(query, parameters, commandType: CommandType.Text);
            }
            return karyakarta;
        }

        public async Task<List<KaryakartaResponse>> GetKaryakartaList(int boothId = 0)
        {
            string query = DBProcedure.GetKaryakartaList;
            using var connection = dapperContext.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("BoothId", boothId);
            return (await connection.QueryAsync<KaryakartaResponse>(query, parameters, commandType: CommandType.StoredProcedure))?.ToList() ?? [];
        }

        public async Task<KaryakartaResponse?> SaveKaryakarta(Karyakarta karyakarta)
        {
            return karyakarta.KaryakartaId > 0
                ? await UpdateKaryakarta(karyakarta)
                : await AddKaryakarta(karyakarta);
        }

        public async Task<KaryakartaFilterResponse> SearchKaryakartaFilter(SearchFilterRequest filterRequest)
        {
            KaryakartaFilterResponse searchResult = new();
            string query = DBProcedure.SearchKaryakartaByFilter;
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

            var response = await connection.QueryAsync<KaryakartaListResponse>(query, sqlParams, commandType: CommandType.StoredProcedure);
            if (response != null && response.Any())
            {
                searchResult.Results = [.. response];
                searchResult.TotalRecord = response.FirstOrDefault()?.TotalRecord ?? 0;
            }
            return searchResult;
        }

        private async Task<KaryakartaResponse?> AddKaryakarta(Karyakarta karyakarta)
        {
            // KaryakartaCode generation and the INSERT share one transaction so a failed insert
            // rolls back the consumed code sequence (no skipped numbers).
            using var connection = dapperContext.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                karyakarta.KaryakartaCode = await entityCodeRepository.GenerateAsync(
                    nameof(EntityType.Karyakarta), connection, transaction);

                var parameters = new DynamicParameters();
                // KaryakartaId is an identity column, so it is excluded from the INSERT.
                string query = CommonUtl.BuildInsertQuery(parameters, karyakarta, KaryakartaTable, dapperContext.Dialect, ["KaryakartaId"]);
                var response = await connection.QueryFirstOrDefaultAsync<KaryakartaResponse>(
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

        private async Task<KaryakartaResponse?> UpdateKaryakarta(Karyakarta karyakarta)
        {
            KaryakartaResponse? response = null;
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                // KaryakartaId drives the WHERE clause; CreatedBy must not be overwritten on update.
                string query = CommonUtl.BuildUpdateQuery(parameters, karyakarta, KaryakartaTable, KaryakartaKey, dapperContext.Dialect, ["CreatedBy"]);
                response = await connection.QueryFirstOrDefaultAsync<KaryakartaResponse>(query, parameters, commandType: CommandType.Text);
            }
            return response;
        }
    }
}
