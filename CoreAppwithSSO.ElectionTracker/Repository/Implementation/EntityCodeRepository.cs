using CoreAppwithSSO.ElectionTracker.Data;
using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Repository.Interface;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CoreAppwithSSO.ElectionTracker.Repository.Implementation
{
    public class EntityCodeRepository(DapperContext dapperContext) : IEntityCodeRepository
    {
        private const string EntityCodeConfigTable = "EntityCodeConfig";
        private const string EntityTypeKey = "EntityType";

        public async Task<string> GenerateAsync(string entityType, CancellationToken ct = default)
        {
            string query = DBProcedure.GetNextEntityCode;
            using var connection = dapperContext.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@EntityType", entityType, DbType.String);
            // Output string params require an explicit size, or SqlClient throws "invalid size of 0".
            parameters.Add("@GeneratedCode", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

            await connection.ExecuteAsync(new CommandDefinition(
                query, parameters, commandType: CommandType.StoredProcedure, cancellationToken: ct));

            return parameters.Get<string>("@GeneratedCode");
        }

        public async Task<EntityCodeConfig?> GetConfigAsync(string entityType, CancellationToken ct = default)
        {
            const string sql = """
            SELECT Id, EntityType, Prefix, Postfix,
                   StartValue, NextSeq, PadLength, IsActive
            FROM   EntityCodeConfig
            WHERE  EntityType = @EntityType
            """;

            EntityCodeConfig? config = null;
            var dialect = dapperContext.Dialect;
            string query = $"SELECT * FROM {dialect.QuoteName(EntityCodeConfigTable)} WHERE {dialect.QuoteName(EntityTypeKey)} = @EntityType";
            using (var connection = dapperContext.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EntityType", entityType, DbType.String);
                config = await connection.QueryFirstOrDefaultAsync<EntityCodeConfig>(query, parameters, commandType: CommandType.Text);
            }
            return config;
        }

        public async Task<int> UpdateConfigAsync(EntityCodeConfig config, CancellationToken ct = default)
        {
            const string sql = """
            UPDATE EntityCodeConfig
            SET Prefix = @Prefix,
                Postfix = @Postfix,
                StartValue = @StartValue,
                NextSeq = @NextSeq,
                PadLength = @PadLength,
                IsActive = @IsActive
            WHERE EntityType = @EntityType
            """;
            using var connection = dapperContext.CreateConnection();
            return await connection.ExecuteAsync(new CommandDefinition(
                sql, config, commandType: CommandType.Text, cancellationToken: ct));
        }
    }
}
