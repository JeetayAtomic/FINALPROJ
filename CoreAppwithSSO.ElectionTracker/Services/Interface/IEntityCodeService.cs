using CoreAppwithSSO.ElectionTracker.Models.Domain;

namespace CoreAppwithSSO.ElectionTracker.Services.Interface
{
    public interface IEntityCodeService
    {
        Task<string?> GenerateAsync(string entityType, CancellationToken ct = default);
        Task<EntityCodeConfig?> GetConfigAsync(string entityType, CancellationToken ct = default);
        Task<int> UpdateConfigAsync(EntityCodeConfig config, CancellationToken ct = default);
    }
}
