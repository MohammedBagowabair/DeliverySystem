using Application.Common.Models;
using Application.DTO.OrderDtos;
using Application.DTO.YearCloseDtos;
using Application.Interfaces;
using AutoMapper;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppPermissions.YearsAccess)]
    public class YearCloseController : ControllerBase
    {
        private readonly IYearCloseService _service;
        private readonly IMapper _mapper;

        public YearCloseController(IYearCloseService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("active")]
        public async Task<ApiResultModel<ActiveYearDTO>> GetActiveAsync()
        {
            try
            {
                var result = await _service.GetActiveYearInfoAsync();
                return new ApiResultModel<ActiveYearDTO>(result);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<ActiveYearDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpGet]
        [Authorize(Roles = AppPermissions.YearsAccess)]
        public async Task<ApiResultModel<List<ClosedBusinessYearDTO>>> GetClosedYearsAsync()
        {
            try
            {
                var years = await _service.GetClosedYearsAsync();
                return new ApiResultModel<List<ClosedBusinessYearDTO>>(years);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<List<ClosedBusinessYearDTO>>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<List<ClosedBusinessYearDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpGet("{year:int}/eligibility")]
        [Authorize(Roles = AppPermissions.YearsAccess)]
        public async Task<ApiResultModel<YearCloseEligibilityDTO>> GetEligibilityAsync(int year)
        {
            try
            {
                var result = await _service.GetEligibilityAsync(year);
                return new ApiResultModel<YearCloseEligibilityDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<YearCloseEligibilityDTO>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<YearCloseEligibilityDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpGet("{year:int}")]
        [Authorize(Roles = AppPermissions.YearsAccess)]
        public async Task<ApiResultModel<YearArchiveDetailDTO>> GetArchiveDetailAsync(int year)
        {
            try
            {
                var result = await _service.GetArchiveDetailAsync(year);
                return new ApiResultModel<YearArchiveDetailDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<YearArchiveDetailDTO>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<YearArchiveDetailDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpGet("{year:int}/profit")]
        [Authorize(Roles = AppPermissions.ViewFinanceAccess)]
        public async Task<ApiResultModel<YearProfitDTO>> GetProfitAsync(int year)
        {
            try
            {
                var result = await _service.GetYearProfitAsync(year);
                return new ApiResultModel<YearProfitDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<YearProfitDTO>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<YearProfitDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpGet("{year:int}/orders")]
        [Authorize(Roles = AppPermissions.YearsAccess)]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> GetOrdersAsync(int year, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var results = await _service.GetYearOrdersAsync(year, page, pageSize);
                return new ApiResultModel<PagedList<OrderDTO>>(_mapper.Map<PagedList<OrderDTO>>(results));
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpPost]
        [Authorize(Roles = AppPermissions.AdminOnly)]
        public async Task<ApiResultModel<ClosedBusinessYearDTO>> CloseYearAsync([FromBody] CloseBusinessYearRequest request)
        {
            try
            {
                var userId = ResolveUserId();
                var result = await _service.CloseYearAsync(request.Year, userId, request.Notes);
                return new ApiResultModel<ClosedBusinessYearDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<ClosedBusinessYearDTO>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<ClosedBusinessYearDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpGet("{year:int}/pdf")]
        [Authorize(Roles = AppPermissions.YearsAccess)]
        public async Task<IActionResult> DownloadAllPdfAsync(int year)
        {
            try
            {
                var bytes = await _service.GenerateAllOrdersPdfAsync(year);
                return File(bytes, "application/pdf", $"Archive_Year_{year}_All.pdf");
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

        [HttpGet("{year:int}/pdf/drivers/{driverId:int}")]
        [Authorize(Roles = AppPermissions.YearsAccess)]
        public async Task<IActionResult> DownloadDriverPdfAsync(int year, int driverId)
        {
            try
            {
                var bytes = await _service.GenerateDriverPdfAsync(year, driverId);
                return File(bytes, "application/pdf", $"Archive_Year_{year}_Driver_{driverId}.pdf");
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
