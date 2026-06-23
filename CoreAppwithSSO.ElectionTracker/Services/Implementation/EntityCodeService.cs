using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Repository.Interface;
using CoreAppwithSSO.ElectionTracker.Services.Interface;

namespace CoreAppwithSSO.ElectionTracker.Services.Implementation
{
    public class EntityCodeService(IEntityCodeRepository entityCodeRepository) : IEntityCodeService
    {
        public async Task<string?> GenerateAsync(string entityType, CancellationToken ct = default)
        {
            var code = await entityCodeRepository.GenerateAsync(entityType, ct);
            return code ?? string.Empty;
        }

        public async Task<EntityCodeConfig?> GetConfigAsync(string entityType, CancellationToken ct = default)
        {
            return await entityCodeRepository.GetConfigAsync(entityType, ct);
        }

        public async Task<int> UpdateConfigAsync(EntityCodeConfig config, CancellationToken ct = default)
        {
            return await entityCodeRepository.UpdateConfigAsync(config, ct);
        }
    }
}
