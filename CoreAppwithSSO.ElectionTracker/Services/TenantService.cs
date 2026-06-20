using CoreAppwithSSO.ElectionTracker.Models;

namespace CoreAppwithSSO.ElectionTracker.Services
{
    public interface ITenantService
    {
        Tenant? GetCurrentTenant();
        void SetCurrentTenant(Tenant tenant);
    }
    public class TenantService : ITenantService
    {
        private Tenant? _currentTenant;
        public Tenant? GetCurrentTenant() => _currentTenant;
        public void SetCurrentTenant(Tenant tenant) => _currentTenant = tenant;
    }
}
