using CoreAppwithSSO.ElectionTracker.Models.Domain;

namespace CoreAppwithSSO.ElectionTracker.Repository.Interface
{
    public interface IEntityCodeRepository
    {
        Task<string> GenerateAsync(string entityType, CancellationToken ct = default);
        Task<EntityCodeConfig?> GetConfigAsync(string entityType, CancellationToken ct = default);
        Task<int> UpdateConfigAsync(EntityCodeConfig config, CancellationToken ct = default);
    }
}
