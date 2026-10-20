using Application.Common.Models;
using Application.DTO.DayCloseDtos;
using Application.Interfaces;
using Domain.Constants;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppPermissions.WorkDayAccess)]
    public class DayCloseController : ControllerBase
    {
        private readonly IDayCloseService _service;

        public DayCloseController(IDayCloseService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ApiResultModel<List<ClosedBusinessDayDTO>>> GetClosedDaysAsync([FromQuery] int take = 60, [FromQuery] int? year = null)
        {
            try
            {
                var days = await _service.GetClosedDaysAsync(take, year);
                return new ApiResultModel<List<ClosedBusinessDayDTO>>(days);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<List<ClosedBusinessDayDTO>>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<List<ClosedBusinessDayDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpGet("eligibility")]
        public async Task<ApiResultModel<DayCloseEligibilityDTO>> GetEligibilityAsync([FromQuery] DateTime businessDate)
        {
            try
            {
                var result = await _service.GetEligibilityAsync(businessDate);
                return new ApiResultModel<DayCloseEligibilityDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<DayCloseEligibilityDTO>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<DayCloseEligibilityDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpGet("{businessDate:datetime}")]
        public async Task<ApiResultModel<DayArchiveDetailDTO>> GetArchiveDetailAsync(DateTime businessDate)
        {
            try
            {
                var result = await _service.GetArchiveDetailAsync(businessDate);
                return new ApiResultModel<DayArchiveDetailDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<DayArchiveDetailDTO>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<DayArchiveDetailDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpGet("is-closed")]
        public async Task<ApiResultModel<bool>> IsClosedAsync([FromQuery] DateTime businessDate)
        {
            try
            {
                var closed = await _service.IsClosedAsync(businessDate);
                return new ApiResultModel<bool>(closed);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<bool>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, false);
            }
        }

        [HttpPost]
        public async Task<ApiResultModel<ClosedBusinessDayDTO>> CloseDayAsync([FromBody] CloseBusinessDayRequest request)
        {
            try
            {
                var userId = ResolveUserId();
                var result = await _service.CloseDayAsync(request.BusinessDate, userId, request.Notes);
                return new ApiResultModel<ClosedBusinessDayDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<ClosedBusinessDayDTO>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<ClosedBusinessDayDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpGet("{businessDate:datetime}/pdf")]
        public async Task<IActionResult> DownloadAllPdfAsync(DateTime businessDate)
        {
            try
            {
                var bytes = await _service.GenerateAllOrdersPdfAsync(businessDate);
                var name = $"Archive_{businessDate:yyyy-MM-dd}_All.pdf";
                return File(bytes, "application/pdf", name);
            }
            catch (DeliveryCoreException ex)
            {
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطأ في إنشاء ملف PDF", error = ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE });
            }
        }

        [HttpGet("{businessDate:datetime}/pdf/drivers/{driverId:int}")]
        public async Task<IActionResult> DownloadDriverPdfAsync(DateTime businessDate, int driverId)
        {
            try
            {
                var bytes = await _service.GenerateDriverPdfAsync(businessDate, driverId);
                var name = $"Archive_{businessDate:yyyy-MM-dd}_Driver_{driverId}.pdf";
                return File(bytes, "application/pdf", name);
            }
            catch (DeliveryCoreException ex)
            {
                return BadRequest(new { message = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "خطأ في إنشاء ملف PDF", error = ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE });
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
