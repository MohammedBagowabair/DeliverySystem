using Application.Common.Models;
using Application.DTO.BusinessDayDtos;
using Application.Interfaces;
using Domain.Constants;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppPermissions.Operations)]
    public class BusinessDayController : ControllerBase
    {
        private readonly IBusinessDayService _service;

        public BusinessDayController(IBusinessDayService service)
        {
            _service = service;
        }

        [HttpGet("current")]
        public async Task<ApiResultModel<BusinessDayStateDTO>> GetCurrentAsync()
        {
            try
            {
                var state = await _service.GetStateAsync();
                return new ApiResultModel<BusinessDayStateDTO>(state);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<BusinessDayStateDTO>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<BusinessDayStateDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpPost("advance")]
        [Authorize(Roles = AppPermissions.WorkDayAccess)]
        public async Task<ApiResultModel<AdvanceBusinessDayResultDTO>> AdvanceAsync(
            [FromBody] AdvanceBusinessDayRequest? request)
        {
            try
            {
                var userId = ResolveUserId();
                var result = await _service.AdvanceToNextDayAsync(
                    userId,
                    request?.Notes,
                    request?.ContinueAnyway ?? false);
                return new ApiResultModel<AdvanceBusinessDayResultDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<AdvanceBusinessDayResultDTO>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<AdvanceBusinessDayResultDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        private int ResolveUserId()
        {
            var claim = User.FindFirst("UserId")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}
