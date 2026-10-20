using Application.Common.Models;
using Application.DTO.OrderDtos;
using Application.Helpers;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Swashbuckle.AspNetCore.Annotations;
using static System.Net.Mime.MediaTypeNames;
using MediatR;
using Application.Commands.Order;
using Application.Queries.Order;
using Application.Commands.Customer;
using Application.DTO.ReportsDto;
using WebApi.Helpers;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppPermissions.OrdersOrReportsAccess)]
    public class OrderController : ControllerBase
    {
        private readonly WeeklyDateRange _weeklyDateRange;
        public IOrderService _service;
        public IMapper _mapper;
        private readonly IPdfGeneratorService _pdfGeneratorService;
        private readonly IMediator _mediator;
        private readonly IBusinessDayService _businessDayService;
        private readonly IYearCloseService _yearCloseService;

        public OrderController(
            IMediator mediator,
            IOrderService service,
            IMapper mapper,
            IPdfGeneratorService pdfGeneratorService,
            WeeklyDateRange weeklyDateRange,
            IBusinessDayService businessDayService,
            IYearCloseService yearCloseService)
        {
            _mediator = mediator;
            _service = service;
            _mapper = mapper;
            _pdfGeneratorService = pdfGeneratorService;
            _weeklyDateRange = weeklyDateRange;
            _businessDayService = businessDayService;
            _yearCloseService = yearCloseService;
        }

        private async Task<(DateTime Start, DateTime End)> GetBusinessTodayRangeAsync()
        {
            var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
            var activeYear = await _yearCloseService.GetActiveYearAsync();
            return OrderDateRanges.Today(businessDate, activeYear);
        }

        private async Task<(DateTime Start, DateTime End)> GetBusinessWeekRangeAsync()
        {
            var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
            var activeYear = await _yearCloseService.GetActiveYearAsync();
            return OrderDateRanges.Last7Days(businessDate, activeYear);
        }

        private async Task<(DateTime Start, DateTime End)> GetBusinessMonthRangeAsync()
        {
            var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
            var activeYear = await _yearCloseService.GetActiveYearAsync();
            return OrderDateRanges.CurrentMonthMtd(businessDate, activeYear);
        }

        private bool RestrictTodayOrdersOnly => StaffAccessHelper.IsTodayOrdersOnly(User);

        private const string TodayOrdersOnlyMessage = "غير مسموح — يمكنك عرض طلبات يوم العمل الحالي فقط";

        private ApiResultModel<T>? DenyHistoricalAccess<T>(T emptyContent = default!)
        {
            if (!RestrictTodayOrdersOnly)
                return null;
            return new ApiResultModel<T>(403, TodayOrdersOnlyMessage, emptyContent);
        }

        private async Task<(DateTime Start, DateTime End)> ResolveOrderRangeAsync(DateTime? startDate, DateTime? endDate)
        {
            var (todayStart, todayEnd) = await GetBusinessTodayRangeAsync();
            return StaffAccessHelper.ClampToBusinessToday(User, startDate, endDate, todayStart, todayEnd);
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = AppPermissions.OrdersAccess)]
        public async Task<ApiResultModel<IEnumerable<OrderDTO>>> GetAllAsync()
        {
            try
            {
                var denied = DenyHistoricalAccess(Enumerable.Empty<OrderDTO>());
                if (denied != null)
                    return denied;

                GetAllOrdersQuery getAllOrdersQuery = new GetAllOrdersQuery();
                var result = await _mediator.Send(getAllOrdersQuery);

                var results = _mapper.Map<IEnumerable<OrderDTO>>(result);


                return new ApiResultModel<IEnumerable<OrderDTO>>(results);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<IEnumerable<OrderDTO>>(ex.Code, ex.Message, []);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "");
                return new ApiResultModel<IEnumerable<OrderDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, []);
            }
        }

        [HttpGet]
        [Authorize(Roles = AppPermissions.OrdersAccess)]
        public async Task<ApiResultModel<OrderDTO>> GetAsync(int id)
        {
            try
            {
                GetOrderByIdQuery getOrderByIdQuery = new(id);
                var result =await _mediator.Send(getOrderByIdQuery);

                var resultOrder =  _mapper.Map<OrderDTO>(result);
                if (resultOrder is null)
                    return new ApiResultModel<OrderDTO>(ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE, "السجل غير موجود", null);

                if (RestrictTodayOrdersOnly)
                {
                    var (todayStart, todayEnd) = await GetBusinessTodayRangeAsync();
                    if (!StaffAccessHelper.IsWithinBusinessToday(resultOrder.DeliveryTime, todayStart, todayEnd))
                        return new ApiResultModel<OrderDTO>(403, TodayOrdersOnlyMessage, null);
                }

                return new ApiResultModel<OrderDTO>(resultOrder);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<OrderDTO>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<OrderDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }

        }


        [HttpPost]
        public async Task<ApiResultModel<OrderDTO>> AddAsync([FromBody] CreateOrderDTO createOrderDTO)
        {
            try
            {
                var order = _mapper.Map<Order>(createOrderDTO);
                var userIdClaim = User.FindFirst("UserId")?.Value
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId) && userId > 0)
                {
                    order.CreatedByUserId = userId;
                }

                CreateOrderCommand createOrderCommand = new CreateOrderCommand(order);

                var result = await _mediator.Send(createOrderCommand);
                var resultDto = _mapper.Map<OrderDTO>(result);
                return new ApiResultModel<OrderDTO>(resultDto);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<OrderDTO>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<OrderDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }
        }

        [HttpGet("PreviousDayPending")]
        public async Task<ApiResultModel<PreviousDayPendingResult>> GetPreviousDayPendingAsync()
        {
            try
            {
                if (RestrictTodayOrdersOnly)
                {
                    var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
                    return new ApiResultModel<PreviousDayPendingResult>(new PreviousDayPendingResult
                    {
                        YemenToday = businessDate,
                        Count = 0,
                        Orders = new List<OrderDTO>()
                    });
                }

                var data = await _service.GetPreviousDayPendingOrdersAsync();
                var result = new PreviousDayPendingResult
                {
                    YemenToday = data.YemenToday,
                    Count = data.Orders.Count,
                    Orders = _mapper.Map<List<OrderDTO>>(data.Orders)
                };
                return new ApiResultModel<PreviousDayPendingResult>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<PreviousDayPendingResult>(ex.Code, ex.Message, null!);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<PreviousDayPendingResult>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null!);
            }
        }

        [HttpGet("PreviousDayPending/Count")]
        public async Task<ApiResultModel<int>> CountPreviousDayPendingAsync()
        {
            try
            {
                if (RestrictTodayOrdersOnly)
                    return new ApiResultModel<int>(0);

                var count = await _service.CountPreviousDayPendingOrdersAsync();
                return new ApiResultModel<int>(count);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<int>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, 0);
            }
        }

        [HttpPost("PreviousDayPending/{id:int}/Resolve")]
        public async Task<ApiResultModel<bool>> ResolvePreviousDayPendingAsync(int id, [FromBody] ResolvePreviousDayOrderDTO dto)
        {
            try
            {
                var denied = DenyHistoricalAccess(false);
                if (denied != null)
                    return denied;

                if (dto == null)
                {
                    return new ApiResultModel<bool>(400, "بيانات الحالة مطلوبة.", false);
                }

                await _service.ResolvePreviousDayPendingAsync(id, dto.NewStatus);
                return new ApiResultModel<bool>(true);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<bool>(ex.Code, ex.Message, false);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<bool>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, false);
            }
        }

        [HttpGet("YemenToday")]
        public async Task<ApiResultModel<object>> GetYemenTodayAsync()
        {
            var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
            var (start, end) = await GetBusinessTodayRangeAsync();
            return new ApiResultModel<object>(new
            {
                yemenToday = YemenTime.Today,
                yemenNow = YemenTime.Now,
                businessDay = businessDate.Date,
                startOfToday = start,
                endOfToday = end
            });
        }

        [HttpDelete]
        [Authorize(Roles = AppPermissions.OrdersAccess)]
        public async Task<ApiResultModel<bool>> DeleteAsync(int id)
        {
            try
            {

                DeleteOrderCommand deleteCustomerCommand = new DeleteOrderCommand(id);
                var result = await _mediator.Send(deleteCustomerCommand);

                return new ApiResultModel<bool>(result);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<bool>(ex.Code, ex.Message, false);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<bool>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, false);
            }

        }
        [HttpPut]
        [Authorize(Roles = AppPermissions.OrdersAccess)]
        public async Task<ApiResultModel<bool>> UpdateAsync(OrderDTO updateOrderDTO)
        {
            try
            {
                var order = _mapper.Map<Order>(updateOrderDTO);
                var allowClosedDayEdit = StaffAccessHelper.IsAdmin(User);
                UpdateOrderCommand command = new UpdateOrderCommand(order, allowClosedDayEdit);
                await _mediator.Send(command);
                return new ApiResultModel<bool>(true);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<bool>(ex.Code, ex.Message, false);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<bool>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, false);
            }


        }


        [HttpGet("GetOrdersPaged")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> GetOrdersPaged(int page = 1, int pageSize = 10)
        {
            try
            {

                return new ApiResultModel<PagedList<OrderDTO>>(_mapper.Map<PagedList<OrderDTO>>(await _service.GetAllPagedAsync(page, pageSize)));

            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<PagedList<OrderDTO>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<PagedList<OrderDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }

        }


        [HttpGet("GetDriverOrders")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> GetDriverOrders(int driverId, string searchTerm, int page = 1, int pageSize = 10, DateTime? startDate = null,DateTime? endDate = null)
        {
            try
            {
                var results = await _service.GetAllOrdersByDriverId(driverId, searchTerm, page, pageSize, startDate, endDate);
                var mappedResults = _mapper.Map<PagedList<OrderDTO>>(results);
                return new ApiResultModel<PagedList<OrderDTO>>(mappedResults);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }
        }


        [HttpGet("TodayOrders")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> TodayOrders(string searchTerm, int page = 1, int pageSize = 10)
        {
            try
            {
                var results = await _service.TodayOrdersAsync(searchTerm, page, pageSize);
                var mappedResults = _mapper.Map<PagedList<OrderDTO>>(results);
                return new ApiResultModel<PagedList<OrderDTO>>(mappedResults);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }
        }
        [HttpGet("GetLastWeekOrders")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> GetLastWeekOrders(string searchTerm, int page = 1, int pageSize = 10, DateTime? startDate = null,
    DateTime? endDate = null)
        {
            try
            {
                var denied = DenyHistoricalAccess<PagedList<OrderDTO>>(null!);
                if (denied != null)
                    return denied;

                var results = await _service.GetLastWeekOrdersAsync(searchTerm, page, pageSize, startDate, endDate);
                var mappedResults = _mapper.Map<PagedList<OrderDTO>>(results);
                return new ApiResultModel<PagedList<OrderDTO>>(mappedResults);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }
        }

        [HttpGet("GetAllOrders")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> GetAllOrders(string searchTerm, int page = 1, int pageSize = 10,[FromQuery] DateTime? startDate = null,[FromQuery] DateTime? endDate = null)

        {
            try
            {
                var (rangeStart, rangeEnd) = await ResolveOrderRangeAsync(startDate, endDate);
                var results = await _service.GetAllOrdersAsync(searchTerm, page, pageSize, rangeStart, rangeEnd);
                var mappedResults = _mapper.Map<PagedList<OrderDTO>>(results);
                return new ApiResultModel<PagedList<OrderDTO>>(mappedResults);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }
        }


        [HttpGet("download-pdf")]
        public async Task<IActionResult> DownloadPdf()
        {
            try
            {
                if (RestrictTodayOrdersOnly)
                    return StatusCode(403, new { message = TodayOrdersOnlyMessage });

                // Fetch the orders from the service
                var pagedOrders = await _service.GetPDFLastWeekOrdersAsync();

                if (pagedOrders?.Entities == null || !pagedOrders.Entities.Any())
                {
                    // If no orders are found, return a "Not Found" response
                    return NotFound("لا توجد طلبات.");
                }

                // Generate the PDF using the PdfGeneratorService
                var pdfBytes = _pdfGeneratorService.GenerateOrderPdf(pagedOrders.Entities, "تقرير آخر 7 أيام");

                // Return the PDF as a file to the client
                return File(pdfBytes, "application/pdf", "LastWeekOrders.pdf");
            }
            catch (Exception ex)
            {
                // If any error occurs during PDF generation, return a server error response
                return StatusCode(500, new { message = "خطأ في إنشاء ملف PDF", error = ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE });
            }
        }

        [HttpGet("download-LastMonth-pdf")]
        public async Task<IActionResult> DownloadLastMonthPdf()
        {
            try
            {
                if (RestrictTodayOrdersOnly)
                    return StatusCode(403, new { message = TodayOrdersOnlyMessage });

                // Fetch the orders from the service
                var pagedOrders = await _service.GetPDFMonthWeekOrdersAsync();

                if (pagedOrders?.Entities == null || !pagedOrders.Entities.Any())
                {
                    // If no orders are found, return a "Not Found" response
                    return NotFound("لا توجد طلبات.");
                }

                // Generate the PDF using the PdfGeneratorService
                var pdfBytes = _pdfGeneratorService.GenerateOrderPdf(pagedOrders.Entities,"تقرير الشهر الحالي");

                // Return the PDF as a file to the client
                return File(pdfBytes, "application/pdf", "LastMonthOrders.pdf");
            }
            catch (Exception ex)
            {
                // If any error occurs during PDF generation, return a server error response
                return StatusCode(500, new { message = "خطأ في إنشاء ملف PDF", error = ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE });
            }
        }


        [HttpGet("LastMonthOrders")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> LastMonthOrders(string searchTerm, int page = 1, int pageSize = 10)
        {
            try
            {
                var denied = DenyHistoricalAccess<PagedList<OrderDTO>>(null!);
                if (denied != null)
                    return denied;

                var results = await _service.LastMonthOrdersAsync(searchTerm, page, pageSize);
                var mappedResults = _mapper.Map<PagedList<OrderDTO>>(results);
                return new ApiResultModel<PagedList<OrderDTO>>(mappedResults);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }
        }


        // Count Today Orders [Processing-Canceled-Dlivered]

        [HttpGet("TotalProcessingOrdersToday")]
        public async Task<ApiResultModel<int>> GetProcessingOrdersToday()
        {
            try
            {
                var (todayStart, todayEnd) = await GetBusinessTodayRangeAsync();
                var totalProcessingOrders = await _service.GetTotalProcessingOrdersAsync(todayStart, todayEnd);
                return new ApiResultModel<int>(totalProcessingOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception)
            {
                return new ApiResultModel<int>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, 0);
            }
        }
        [HttpGet("TotalDliveredOrdersToday")]
        public async Task<ApiResultModel<int>> GetDliveredOrdersToday()
        {
            try
            {
                var (todayStart, todayEnd) = await GetBusinessTodayRangeAsync();
                var totalProcessingOrders = await _service.GetTotalDileveredOrdersAsync(todayStart, todayEnd);
                return new ApiResultModel<int>(totalProcessingOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception)
            {
                return new ApiResultModel<int>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, 0);
            }
        }
        [HttpGet("TotalCanceledOrdersToday")]
        public async Task<ApiResultModel<int>> GetCanceledOrdersToday()
        {
            try
            {
                var (todayStart, todayEnd) = await GetBusinessTodayRangeAsync();
                var totalProcessingOrders = await _service.GetTotalCanceledOrdersAsync(todayStart, todayEnd);
                return new ApiResultModel<int>(totalProcessingOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception)
            {
                return new ApiResultModel<int>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, 0);
            }
        }



        // Count Last Week Orders [Processing-Canceled-Dlivered]

        [HttpGet("TotalProcessingOrdersLastWeek")]
        public async Task<ApiResultModel<int>> GetProcessingOrdersLastWeek()
        {
            try
            {
                var denied = DenyHistoricalAccess(0);
                if (denied != null)
                    return denied;

                var (start, end) = await GetBusinessWeekRangeAsync();
                var totalProcessingOrders = await _service.GetTotalProcessingOrdersAsync(start, end);
                return new ApiResultModel<int>(totalProcessingOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception)
            {
                return new ApiResultModel<int>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, 0);
            }
        }
        [HttpGet("TotalDliveredOrdersLastWeek")]
        public async Task<ApiResultModel<int>> GetDliveredOrdersLastWeek()
        {
            try
            {
                var denied = DenyHistoricalAccess(0);
                if (denied != null)
                    return denied;

                var (start, end) = await GetBusinessWeekRangeAsync();
                var totalProcessingOrders = await _service.GetTotalDileveredOrdersAsync(start, end);
                return new ApiResultModel<int>(totalProcessingOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception)
            {
                return new ApiResultModel<int>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, 0);
            }
        }
        [HttpGet("TotalCanceledOrdersLastWeek")]
        public async Task<ApiResultModel<int>> GetCanceledOrdersLastWeek()
        {
            try
            {
                var denied = DenyHistoricalAccess(0);
                if (denied != null)
                    return denied;

                var (start, end) = await GetBusinessWeekRangeAsync();
                var totalProcessingOrders = await _service.GetTotalCanceledOrdersAsync(start, end);
                return new ApiResultModel<int>(totalProcessingOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception)
            {
                return new ApiResultModel<int>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, 0);
            }
        }



        // Count Last Month Orders [Processing-Canceled-Dlivered]

        [HttpGet("TotalProcessingOrdersLastMonth")]
        public async Task<ApiResultModel<int>> GetProcessingOrdersLastMonth()
        {
            try
            {
                var denied = DenyHistoricalAccess(0);
                if (denied != null)
                    return denied;

                var (start, end) = await GetBusinessMonthRangeAsync();
                var totalProcessingOrders = await _service.GetTotalProcessingOrdersAsync(start, end);
                return new ApiResultModel<int>(totalProcessingOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception)
            {
                return new ApiResultModel<int>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, 0);
            }
        }
        [HttpGet("TotalDliveredOrdersLastMonth")]
        public async Task<ApiResultModel<int>> GetDliveredOrdersLastMonth()
        {
            try
            {
                var denied = DenyHistoricalAccess(0);
                if (denied != null)
                    return denied;

                var (start, end) = await GetBusinessMonthRangeAsync();
                var totalProcessingOrders = await _service.GetTotalDileveredOrdersAsync(start, end);
                return new ApiResultModel<int>(totalProcessingOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception)
            {
                return new ApiResultModel<int>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, 0);
            }
        }
        [HttpGet("TotalCanceledOrdersLastMonth")]
        public async Task<ApiResultModel<int>> GetCanceledOrdersLastMonth()
        {
            try
            {
                var denied = DenyHistoricalAccess(0);
                if (denied != null)
                    return denied;

                var (start, end) = await GetBusinessMonthRangeAsync();
                var totalProcessingOrders = await _service.GetTotalCanceledOrdersAsync(start, end);
                return new ApiResultModel<int>(totalProcessingOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception)
            {
                return new ApiResultModel<int>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, 0);
            }
        }


        //
        [HttpGet("GetAllFiltered-Orders")]
        public async Task<ApiResultModel<PagedList<Order>>> GetFilteredOrders(string searchTerm = null,int page = 1,int pageSize = 10,[FromQuery] DateTime? startDate = null,[FromQuery] DateTime? endDate = null,[FromQuery] string selectedDriver = null,[FromQuery] OrderStatus? selectedStatus = null,[FromQuery] int? createdByUserId = null)
        {
            try
            {
                var (rangeStart, rangeEnd) = await ResolveOrderRangeAsync(startDate, endDate);

                // Call the service with the provided parameters
                var results = await _service.GetAllOrdersFiltredAsync(
                    page,
                    pageSize,
                    searchTerm,
                    rangeStart,
                    rangeEnd,
                    selectedDriver,
                    selectedStatus,
                    createdByUserId);

                // Return the results directly, as the service already provides the data in the expected format
                return new ApiResultModel<PagedList<Order>>(results);
            }
            catch (DeliveryCoreException ex)
            {
                // Return an error with the custom exception's code and message
                return new ApiResultModel<PagedList<Order>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                // Return a general error for unexpected exceptions
                return new ApiResultModel<PagedList<Order>>(500, $"حدث خطأ غير متوقع: {ex.Message}", null);
            }
        }

        [HttpGet("DownloadFilteredDataPdf")]
        public async Task<IActionResult> DownloadFilteredDataPdf(string searchTerm = null, int page = 1, int pageSize = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string selectedDriver = null, [FromQuery] OrderStatus? selectedStatus = null, [FromQuery] int? createdByUserId = null)
        {
            try
            {
                return await BuildOrdersPdfAsync(searchTerm, page, pageSize, startDate, endDate, selectedDriver, selectedStatus, createdByUserId);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "خطأ في إنشاء ملف PDF", error = ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE });
            }
        }

        /// <summary>Reports page download — same PDF as filtered orders, accepts int selectedDriver.</summary>
        [HttpGet("DownloadReportsPdf")]
        public async Task<IActionResult> DownloadReportsPdf(
            string searchTerm = null,
            int page = 1,
            int pageSize = 10000,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? selectedDriver = null,
            [FromQuery] OrderStatus? selectedStatus = null,
            [FromQuery] int? createdByUserId = null)
        {
            try
            {
                var driverKey = selectedDriver is > 0 ? selectedDriver.Value.ToString() : null;
                return await BuildOrdersPdfAsync(searchTerm, page, pageSize, startDate, endDate, driverKey, selectedStatus, createdByUserId);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "خطأ في إنشاء ملف PDF", error = ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE });
            }
        }

        private async Task<IActionResult> BuildOrdersPdfAsync(
            string? searchTerm,
            int page,
            int pageSize,
            DateTime? startDate,
            DateTime? endDate,
            string? selectedDriver,
            OrderStatus? selectedStatus,
            int? createdByUserId)
        {
            var (rangeStart, rangeEnd) = await ResolveOrderRangeAsync(startDate, endDate);

            var results = await _service.GetAllOrdersFiltredAsync(
                page,
                pageSize,
                searchTerm,
                rangeStart,
                rangeEnd,
                selectedDriver,
                selectedStatus,
                createdByUserId);

            if (results?.Entities == null || !results.Entities.Any())
                return NotFound(new { message = "لا توجد طلبات ضمن الفترة المحددة." });

            string name;
            string reportTitle = "تقرير الطلبات المسلّمة";
            if (startDate.HasValue || endDate.HasValue)
            {
                var from = startDate?.ToString("dd/MM/yyyy") ?? "…";
                var to = endDate?.ToString("dd/MM/yyyy") ?? "…";
                reportTitle = $"تقرير الطلبات ({from} – {to})";
            }

            if (!string.IsNullOrEmpty(selectedDriver) && int.TryParse(selectedDriver, out var driverId))
            {
                name = results.Entities
                    .Where(e => e.DriverId == driverId || e.Driver?.Id == driverId)
                    .Select(e => !string.IsNullOrWhiteSpace(e.DriverName) ? e.DriverName : e.Driver?.FullName)
                    .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                    ?? $"موصل #{selectedDriver}";
            }
            else
            {
                name = "جميع الموصلين";
            }

            var pdfBytes = _pdfGeneratorService.GenerateOrderPdf(results.Entities.ToList(), reportTitle, name);
            var fileName = $"OrdersReport_{YemenTime.Today:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        //[HttpGet("DownloadReportsPdf-legacy")]
        //public async Task<IActionResult> DownloadReportsPdfLegacy(...)
        //{
        //}

        [HttpGet("GetProfitA-Revenu")]
        [Authorize(Roles = AppPermissions.ViewFinanceAccess)]
        public async Task<ApiResultModel<DriverReportResult>> GetProfitAndRevenu(int? DriverId, string searchTerm = null, int page = 1, int pageSize = 5, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int? selectedDriver = null, [FromQuery] OrderStatus? selectedStatus = null, [FromQuery] int? createdByUserId = null)
        {
            try
            {
                var (rangeStart, rangeEnd) = await ResolveOrderRangeAsync(startDate, endDate);
                var results = await _service.GetDriverReportsAsync(DriverId, page, pageSize, searchTerm, rangeStart, rangeEnd, selectedDriver, selectedStatus, createdByUserId);
                return new ApiResultModel<DriverReportResult>(results ?? new DriverReportResult());
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<DriverReportResult>(ex.Code, ex.Message, new DriverReportResult());
            }
            catch (Exception)
            {
                return new ApiResultModel<DriverReportResult>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, new DriverReportResult());
            }
        }

        [HttpGet("suggestions/delivery-locations")]
        public async Task<ApiResultModel<IReadOnlyList<string>>> GetDeliveryLocationSuggestions([FromQuery] string? search = null, [FromQuery] int limit = 15)
        {
            try
            {
                var results = await _service.GetDeliveryLocationSuggestionsAsync(search, limit);
                return new ApiResultModel<IReadOnlyList<string>>(results);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<IReadOnlyList<string>>(ex.Code, ex.Message, Array.Empty<string>());
            }
            catch (Exception)
            {
                return new ApiResultModel<IReadOnlyList<string>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, Array.Empty<string>());
            }
        }

        [HttpGet("suggestions/notes")]
        public async Task<ApiResultModel<IReadOnlyList<string>>> GetOrderNoticeSuggestions([FromQuery] string? search = null, [FromQuery] int limit = 15)
        {
            try
            {
                var results = await _service.GetOrderNoticeSuggestionsAsync(search, limit);
                return new ApiResultModel<IReadOnlyList<string>>(results);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<IReadOnlyList<string>>(ex.Code, ex.Message, Array.Empty<string>());
            }
            catch (Exception)
            {
                return new ApiResultModel<IReadOnlyList<string>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, Array.Empty<string>());
            }
        }

        
    }

   
}
