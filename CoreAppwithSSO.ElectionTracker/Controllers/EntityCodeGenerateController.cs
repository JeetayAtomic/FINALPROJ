using CoreAppwithSSO.ElectionTracker.Constants;
using CoreAppwithSSO.ElectionTracker.Models.Domain;
using CoreAppwithSSO.ElectionTracker.Services.Implementation;
using CoreAppwithSSO.ElectionTracker.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.ElectionTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityCodeGenerateController(IEntityCodeService entityCodeService) : BaseController
    {
        [HttpPost(UrlConstant.GenerateEntityCode)]
        public async Task<IActionResult> GenerateEntityCode(EntityCodeRequest request)
        {
            var response = await HandleGetAsync(
                                              () => entityCodeService.GenerateAsync(request.EntityType),
                                              Constant.SAVE_ENTITY_CODE_SUCCESS,
                                              Constant.SAVE_ENTITY_CODE_ERROR,
                                              result => string.IsNullOrEmpty(result));

            return Ok(response);
        }

        [HttpGet(UrlConstant.GetEntityCodeConfig)]
        public async Task<IActionResult> GetEntityCodeConfig(string entityType)
        {
            var response = await HandleGetAsync(
                () => entityCodeService.GetConfigAsync(entityType),
                Constant.GET_ENTITY_CODE_CONFIG_SUCCESS,
                Constant.GET_ENTITY_CODE_CONFIG_ERROR
            );
            return Ok(response);
        }

        [HttpPut(UrlConstant.UpdateEntityCodeConfig)]
        public async Task<IActionResult> UpdateEntityCodeConfig(EntityCodeConfig request)
        {
            var response = await HandleUpdateAsync(
                () => entityCodeService.UpdateConfigAsync(request),
                Constant.UPDATE_ENTITY_CODE_CONFIG_SUCCESS,
                Constant.UPDATE_ENTITY_CODE_CONFIG_ERROR,
                rowsAffected => rowsAffected == 0
            );
            return Ok(response);
        }
    }

    public class EntityCodeRequest
    {
        public string EntityType { get; set; } = string.Empty;
    }
}
