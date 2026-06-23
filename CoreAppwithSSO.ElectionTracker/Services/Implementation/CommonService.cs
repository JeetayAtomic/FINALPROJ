using ATM.Core.ISC.Abstractions;
using CoreAppwithSSO.ElectionTracker.Common;
using CoreAppwithSSO.ElectionTracker.Models;
using CoreAppwithSSO.ElectionTracker.Services.Interface;

namespace CoreAppwithSSO.ElectionTracker.Services.Implementation
{
    public class CommonService : ICommonService
    {
        private readonly IISCClient _isc;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CommonService(IISCClient isc, IHttpContextAccessor httpContextAccessor)
        {
            _isc = isc;
            _isc.TenantId = httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].ToString() ?? "";
            _httpContextAccessor = httpContextAccessor;
        }

        //public async Task<List<ProfileOptionValueResponse>> GetProfileOptionValues(ProfileOptionRequest profileOption)
        //{
        //    var resp = await _isc.PostAsync<ProfileOptionRequest, ATMResponse<List<ProfileOptionValueResponse>>>(
        //        "api/common/GetProfileOptionValues", profileOption, "CommonAPIBase");

        //    if (resp != null || !resp.IsError && resp.Data != null)
        //    {
        //        return resp.Data.FirstOrDefault() ?? [];
        //    }
        //    return [];
        //}
    }
}
