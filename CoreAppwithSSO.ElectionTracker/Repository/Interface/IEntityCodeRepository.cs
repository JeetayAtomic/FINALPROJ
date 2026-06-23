using CoreAppwithSSO.ElectionTracker.Models.Domain;
using System.Data;

namespace CoreAppwithSSO.ElectionTracker.Repository.Interface
{
    public interface IEntityCodeRepository
    {
        Task<string> GenerateAsync(string entityType, CancellationToken ct = default);
        Task<string> GenerateAsync(string entityType, IDbConnection connection,
                                   IDbTransaction? transaction, CancellationToken ct = default);
        Task<EntityCodeConfig?> GetConfigAsync(string entityType, CancellationToken ct = default);
        Task<int> UpdateConfigAsync(EntityCodeConfig config, CancellationToken ct = default);
    }
}
