
using Application.DTO.OrderDtos;
using Application.DTO.ReportsDto;
using Application.Helpers;
using Application.Interfaces;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        public  IApplicationDbContext _dbContext;
        private readonly ICompanyFundService _companyFundService;
        private readonly IDayCloseService _dayCloseService;
        private readonly IYearCloseService _yearCloseService;
        private readonly IBusinessDayService _businessDayService;

        public OrderService(
            IApplicationDbContext dbContext,
            ICompanyFundService companyFundService,
            IDayCloseService dayCloseService,
            IYearCloseService yearCloseService,
            IBusinessDayService businessDayService)
        {
            _dbContext = dbContext;
            _companyFundService = companyFundService;
            _dayCloseService = dayCloseService;
            _yearCloseService = yearCloseService;
            _businessDayService = businessDayService;
        }
        
        public async Task<IEnumerable<Order>> GetAll()
        {
            return await _dbContext.GetAsync<Order>();
        }

        public async Task<Order> GetById(int id)
        {
            return (await _dbContext.GetAsync<Order>(
                x => x.Id == id,
                q => q.Include(o => o.Customer)
                      .Include(o => o.Driver)
                      .Include(o => o.CreatedByUser)))?.FirstOrDefault();
        }
        public async Task<Order> Create(Order order)
        {
            order.orderStatus = OrderStatus.Processing;

            var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();

            // New orders belong to the controlled current business day (not calendar clock).
            // Explicit past DeliveryTime is kept for forgotten historical backfill (closed-day check below).
            if (order.DeliveryTime != default && order.DeliveryTime.Date < businessDate)
            {
                order.DeliveryTime = YemenTime.NoonOn(order.DeliveryTime);
            }
            else
            {
                // default, same day, or calendar-ahead (e.g. Aug 22 while business day is still Aug 21)
                order.DeliveryTime = businessDate == YemenTime.Today
                    ? YemenTime.Now
                    : YemenTime.NoonOn(businessDate);
            }

            if (await _dayCloseService.IsClosedAsync(order.DeliveryTime.Date))
            {
                throw new DeliveryCoreException(
                    $"لا يمكن إضافة طلبات ليوم {order.DeliveryTime:yyyy/MM/dd} لأنه مغلق ومؤرشف.",
                    ErrorCodes.DB_UNABLE_TO_INSERT_RECORD_CODE,
                    null);
            }

            if (await _yearCloseService.IsClosedAsync(order.DeliveryTime.Year))
            {
                throw new DeliveryCoreException(
                    $"لا يمكن إضافة طلبات لسنة {order.DeliveryTime.Year} لأنها مغلقة ومؤرشفة.",
                    ErrorCodes.DB_UNABLE_TO_INSERT_RECORD_CODE,
                    null);
            }

            await ApplyPartyNamesAsync(order);
            await ApplyRevenueSnapshotAsync(order);

            return await _dbContext.AddAsync<Order>(order);
        }
        public async Task Update(Order order, bool allowClosedDayEdit = false)
        {
            var existing = await GetById(order.Id)
                ?? throw new DeliveryCoreException(ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE);

            if (!OrderStatusTransitions.CanTransition(existing.orderStatus, order.orderStatus))
            {
                throw new DeliveryCoreException(
                    $"لا يمكن تغيير حالة الطلب من {existing.orderStatus} إلى {order.orderStatus}. المسموح: {OrderStatusTransitions.DescribeAllowed(existing.orderStatus)}.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var orderDay = existing.DeliveryTime.Date;
            var dayClosed = await _dayCloseService.IsClosedAsync(orderDay);
            if (dayClosed && !allowClosedDayEdit)
            {
                throw new DeliveryCoreException(
                    $"لا يمكن تعديل الطلب بعد إغلاق يوم {orderDay:yyyy/MM/dd}. التعديل بعد الإغلاق متاح للمدير فقط.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            if (await _yearCloseService.IsClosedAsync(orderDay.Year))
            {
                throw new DeliveryCoreException(
                    $"لا يمكن تعديل الطلب بعد إغلاق سنة {orderDay.Year}.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            order.CustomerId = order.CustomerId == 0 ? existing.CustomerId : order.CustomerId;
            order.DriverId = order.DriverId == 0 ? existing.DriverId : order.DriverId;
            order.CreatedByUserId = existing.CreatedByUserId;
            order.CustomerName = existing.CustomerName;
            order.DriverName = existing.DriverName;

            await EnsureDriverAssignedForDeliveryAsync(order.DriverId, order.orderStatus);

            // Preserve historical DeliveryTime so previous-day deliveries stay on that day's reports.
            // Only stamp Yemen now when delivering and the existing time is missing.
            if (existing.orderStatus != OrderStatus.Delivered && order.orderStatus == OrderStatus.Delivered)
            {
                order.DeliveryTime = existing.DeliveryTime != default
                    ? existing.DeliveryTime
                    : YemenTime.Now;
            }
            else if (order.orderStatus == OrderStatus.Processing && order.DeliveryTime == default)
            {
                order.DeliveryTime = existing.DeliveryTime;
            }
            else if (order.orderStatus != OrderStatus.Processing)
            {
                order.DeliveryTime = existing.DeliveryTime;
            }

            if (order.CustomerId != existing.CustomerId || order.DriverId != existing.DriverId
                || string.IsNullOrWhiteSpace(order.CustomerName) || string.IsNullOrWhiteSpace(order.DriverName))
            {
                await ApplyPartyNamesAsync(order);
            }

            await ApplyRevenueSnapshotAsync(order);

            _dbContext._dbContext.Set<Order>().Update(order);
            await _dbContext._dbContext.SaveChangesAsync();

            // Keep closed-day archive totals accurate when Admin changes status after close.
            if (dayClosed && allowClosedDayEdit)
                await _dayCloseService.RefreshClosedDayTotalsAsync(orderDay);
        }

        public async Task ResolvePreviousDayPendingAsync(int orderId, OrderStatus newStatus)
        {
            if (newStatus is not (OrderStatus.Delivered or OrderStatus.Canceled))
            {
                throw new DeliveryCoreException(
                    "يُسمح فقط بتسليم أو رفض الطلبات المعلقة من أيام سابقة.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var existing = await GetById(orderId)
                ?? throw new DeliveryCoreException(ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE);

            if (existing.orderStatus != OrderStatus.Processing)
            {
                throw new DeliveryCoreException(
                    "الطلب ليس معلقاً (قيد المعالجة).",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
            if (existing.DeliveryTime.Date >= businessDate)
            {
                throw new DeliveryCoreException(
                    "هذا الطلب ليس من يوم عمل سابق.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            await EnsureDriverAssignedForDeliveryAsync(existing.DriverId, newStatus);

            existing.orderStatus = newStatus;
            if (string.IsNullOrWhiteSpace(existing.DriverName) || string.IsNullOrWhiteSpace(existing.CustomerName))
                await ApplyPartyNamesAsync(existing);

            // Keep original DeliveryTime for that historical day's financials
            _dbContext._dbContext.Set<Order>().Update(existing);
            await _dbContext._dbContext.SaveChangesAsync();
        }

        private async Task ApplyRevenueSnapshotAsync(Order order)
        {
            var rate = RevenueSplit.DefaultDriverCommissionRate;
            if (order.DriverId > 0)
            {
                var driverRate = await _dbContext._dbContext.Set<Driver>()
                    .AsNoTracking()
                    .Where(d => d.Id == order.DriverId)
                    .Select(d => d.CommissionRate)
                    .FirstOrDefaultAsync();
                if (driverRate > 0)
                    rate = driverRate;
            }

            OrderRevenueCalculator.ApplyToOrder(order, rate);
        }

        private async Task ApplyPartyNamesAsync(Order order)
        {
            if (order.CustomerId > 0)
            {
                var customerName = await _dbContext._dbContext.Set<Customer>()
                    .AsNoTracking()
                    .Where(c => c.Id == order.CustomerId)
                    .Select(c => c.FullName)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(customerName))
                    order.CustomerName = customerName;
            }

            if (order.DriverId > 0)
            {
                var driverName = await _dbContext._dbContext.Set<Driver>()
                    .AsNoTracking()
                    .Where(d => d.Id == order.DriverId)
                    .Select(d => d.FullName)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(driverName))
                    order.DriverName = driverName;
            }

            order.CustomerName ??= string.Empty;
            order.DriverName ??= string.Empty;
        }

        private async Task EnsureDriverAssignedForDeliveryAsync(int driverId, OrderStatus targetStatus)
        {
            if (targetStatus != OrderStatus.Delivered)
                return;

            if (driverId <= 0)
            {
                throw new DeliveryCoreException(
                    "لا يمكن تسليم الطلب بدون تحديد موصل. يرجى اختيار موصل أولاً.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var driverExists = await _dbContext._dbContext.Set<Driver>()
                .AsNoTracking()
                .AnyAsync(d => d.Id == driverId);

            if (!driverExists)
            {
                throw new DeliveryCoreException(
                    "الموصل المحدد غير موجود. يرجى اختيار موصل صالح قبل التسليم.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }
        }

        public async Task<PreviousDayPendingOrdersData> GetPreviousDayPendingOrdersAsync()
        {
            var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
            var startOfBusinessDay = YemenTime.StartOfDay(businessDate);
            var orders = await _dbContext._dbContext.Set<Order>()
                .AsNoTracking()
                .Include(o => o.Driver)
                .Include(o => o.Customer)
                .Include(o => o.CreatedByUser)
                .Where(o =>
                    o.orderStatus == OrderStatus.Processing &&
                    o.DeliveryTime < startOfBusinessDay)
                .OrderBy(o => o.DeliveryTime)
                .ThenBy(o => o.Id)
                .ToListAsync();

            return new PreviousDayPendingOrdersData
            {
                YemenToday = businessDate,
                Orders = orders
            };
        }

        public async Task<int> CountPreviousDayPendingOrdersAsync()
        {
            var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
            var startOfBusinessDay = YemenTime.StartOfDay(businessDate);
            return await _dbContext._dbContext.Set<Order>()
                .CountAsync(o =>
                    o.orderStatus == OrderStatus.Processing &&
                    o.DeliveryTime < startOfBusinessDay);
        }

        public async Task<bool> Delete(int id)
        {
            var order = await _dbContext._dbContext.Set<Order>()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new DeliveryCoreException(ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE);

            var businessDate = order.DeliveryTime.Date;
            if (await _dayCloseService.IsClosedAsync(businessDate))
            {
                throw new DeliveryCoreException(
                    $"لا يمكن حذف طلب ليوم {businessDate:yyyy/MM/dd} لأنه مغلق ومؤرشف. الحذف يؤثر على حسابات الربح.",
                    ErrorCodes.NOT_ALL_RECORDS_DELETED_CODE,
                    null);
            }

            if (await _yearCloseService.IsClosedAsync(order.DeliveryTime.Year))
            {
                throw new DeliveryCoreException(
                    $"لا يمكن حذف طلب لسنة {order.DeliveryTime.Year} لأنها مغلقة ومؤرشفة. الحذف يؤثر على حسابات الربح.",
                    ErrorCodes.NOT_ALL_RECORDS_DELETED_CODE,
                    null);
            }

            return await _dbContext.DeleteAsync<Order>(id);
        }
        public async Task<int> CountAsync()
        {
            return await _dbContext.CountAsync<Order>();
        }
        public async Task<PagedList<Order>> GetAllPagedAsync(int page, int PageSize)
        {
            return await _dbContext.GetPagedAsync<Order>(page, PageSize, null, ["Driver", "Customer"]);
        }

        public async Task<PagedList<Order>> TodayOrdersAsync(string searchTerm,int page,int pageSize,DateTime? startDate = null,DateTime? endDate = null)
        {
            searchTerm = searchTerm?.Trim()?.ToLower();

            var activeYear = await _yearCloseService.GetActiveYearAsync();
            var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
            var today = OrderDateRanges.Today(businessDate, activeYear);
            DateTime start = startDate ?? today.Start;
            DateTime end = endDate ?? today.End;

            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end
                    );

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(order =>
                    order.Customer.FullName.ToLower().Contains(searchTerm) ||
                    order.Driver.FullName.ToLower().Contains(searchTerm) ||
                    order.Title.ToLower().Contains(searchTerm));
            }

            int totalCount = await query.CountAsync();

            var orders = await query
                .OrderBy(order => order.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<Order>(totalCount, orders, page, pageSize);
        }
        public async Task<PagedList<Order>> GetAllOrdersByDriverId(int driverId,string searchTerm,int page,int pageSize,DateTime? startDate = null,DateTime? endDate = null)
        {
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();

            // Set default date range
            DateTime start = startDate ?? DateTime.Today;
            DateTime end = endDate ?? DateTime.Now.AddDays(1);

            // Build the query
            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DriverId == driverId &&
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end&&
                    order.orderStatus==OrderStatus.Delivered &&
                    order.orderStatus == OrderStatus.Delivered);

            // Apply search term if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(order =>
                    order.Customer.FullName.ToLower().Contains(searchTerm) ||
                    order.Driver.FullName.ToLower().Contains(searchTerm) ||
                    order.Title.ToLower().Contains(searchTerm));
            }

            // Get total count for pagination
            int totalCount = await query.CountAsync();

            // Apply pagination
            var orders = await query
                .OrderBy(order => order.Id) // or use OrderByDescending if required
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return the paged list
            return new PagedList<Order>(totalCount, orders, page, pageSize);
        }

        public async Task<PagedList<Order>> GetLastWeekOrdersAsync(string searchTerm,int page,int pageSize,DateTime? startDate = null,DateTime? endDate = null)
        {
            searchTerm = searchTerm?.Trim()?.ToLower();

            var activeYear = await _yearCloseService.GetActiveYearAsync();
            var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
            var week = OrderDateRanges.Last7Days(businessDate, activeYear);
            DateTime start = startDate ?? week.Start;
            DateTime end = endDate ?? week.End;

            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(order =>
                    (order.CustomerName != null && order.CustomerName.ToLower().Contains(searchTerm)) ||
                    (order.DriverName != null && order.DriverName.ToLower().Contains(searchTerm)) ||
                    (order.Customer != null && order.Customer.FullName.ToLower().Contains(searchTerm)) ||
                    (order.Driver != null && order.Driver.FullName.ToLower().Contains(searchTerm)) ||
                    order.Title.ToLower().Contains(searchTerm));
            }

            int totalCount = await query.CountAsync();

            var orders = await query
                .OrderBy(order => order.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<Order>(totalCount, orders, page, pageSize);
        }
        public async Task<PagedList<Order>> GetAllOrdersAsync(string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                searchTerm = searchTerm?.Trim()?.ToLower();

                if (startDate == null && endDate == null)
                {
                    var activeYear = await _yearCloseService.GetActiveYearAsync();
                    var yearBounds = OrderDateRanges.Year(activeYear);
                    startDate = yearBounds.Start;
                    endDate = yearBounds.End;
                }

                DateTime start = startDate ?? DateTime.MinValue;
                DateTime end = endDate ?? DateTime.MaxValue;

                if (string.IsNullOrEmpty(searchTerm))
                {
                    return await _dbContext.GetPagedAsync<Order>(
                        page,
                        pageSize,
                        x => x.DeliveryTime >= start && x.DeliveryTime <= end,
                        ["Driver", "Customer"],
                        true);
                }

                return await _dbContext.GetPagedAsync<Order>(
                    page,
                    pageSize,
                    d => d.DeliveryTime >= start
                         && d.DeliveryTime <= end
                         && ((d.CustomerName != null && d.CustomerName.ToLower().Contains(searchTerm))
                             || (d.DriverName != null && d.DriverName.ToLower().Contains(searchTerm))
                             || (d.Customer != null && d.Customer.FullName.ToLower().Contains(searchTerm))
                             || (d.Driver != null && d.Driver.FullName.ToLower().Contains(searchTerm))
                             || d.Title.ToLower().Contains(searchTerm)),
                    ["Driver", "Customer"],
                    true);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<PagedList<Order>> GetPDFLastWeekOrdersAsync()
        {
            try
            {
                var activeYear = await _yearCloseService.GetActiveYearAsync();
                var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
                var week = OrderDateRanges.Last7Days(businessDate, activeYear);
                var query = _dbContext._dbContext.Set<Order>()
                    .Include(order => order.Driver)
                    .Include(order => order.Customer)
                    .Where(order =>
                        order.DeliveryTime >= week.Start &&
                        order.DeliveryTime <= week.End &&
                        order.orderStatus == OrderStatus.Delivered);

                var orders = await query.ToListAsync();
                return new PagedList<Order>(orders.Count, orders, 1, Math.Max(orders.Count, 1));
            }
            catch (Exception ex)
            {
                throw new Exception("خطأ في جلب الطلبات: " + ex.Message);
            }
        }
        public async Task<PagedList<Order>> GetPDFMonthWeekOrdersAsync()
        {
            try
            {
                var activeYear = await _yearCloseService.GetActiveYearAsync();
                var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
                var month = OrderDateRanges.CurrentMonthMtd(businessDate, activeYear);
                var query = _dbContext._dbContext.Set<Order>()
                    .Include(order => order.Driver)
                    .Include(order => order.Customer)
                    .Where(order =>
                        order.DeliveryTime >= month.Start &&
                        order.DeliveryTime <= month.End &&
                        order.orderStatus == OrderStatus.Delivered);

                var orders = await query.ToListAsync();
                return new PagedList<Order>(orders.Count, orders, 1, Math.Max(orders.Count, 1));
            }
            catch (Exception ex)
            {
                throw new Exception("خطأ في جلب الطلبات: " + ex.Message);
            }
        }
        public async Task<PagedList<Order>> LastMonthOrdersAsync(string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            searchTerm = searchTerm?.Trim()?.ToLower();

            var activeYear = await _yearCloseService.GetActiveYearAsync();
            var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
            var month = OrderDateRanges.CurrentMonthMtd(businessDate, activeYear);
            DateTime start = startDate ?? month.Start;
            DateTime end = endDate ?? month.End;

            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end
                    );

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(order =>
                    order.Customer.FullName.ToLower().Contains(searchTerm) ||
                    order.Driver.FullName.ToLower().Contains(searchTerm) ||
                    order.Title.ToLower().Contains(searchTerm));
            }

            int totalCount = await query.CountAsync();

            var orders = await query
                .OrderBy(order => order.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<Order>(totalCount, orders, page, pageSize);
        }


        public async Task<int> GetTotalProcessingOrdersAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime? Start = startDate;
            DateTime? End = endDate;

            return await _dbContext.CountAsync<Order>(
                order => order.orderStatus == OrderStatus.Processing &&
                         order.DeliveryTime >= Start &&
                         order.DeliveryTime <= End);
        }
        public async Task<int> GetTotalDileveredOrdersAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime? Start = startDate;
            DateTime? End = endDate;

            return await _dbContext.CountAsync<Order>(
                order => order.orderStatus == OrderStatus.Delivered &&
                          order.DeliveryTime >= Start &&
                         order.DeliveryTime <= End);
        }
        public async Task<int> GetTotalCanceledOrdersAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime? Start = startDate;
            DateTime? End = endDate;

            return await _dbContext.CountAsync<Order>(
                order => order.orderStatus == OrderStatus.Canceled &&
                          order.DeliveryTime >= Start &&
                         order.DeliveryTime <= End);
        }

        // Filtered Data

        public async Task<PagedList<Order>> GetAllOrdersFiltredAsync(
    int page,
    int pageSize,
    string searchTerm = null,
    DateTime? startDate = null,
    DateTime? endDate = null,
    string SelectedDriver = null,
    OrderStatus? SelectedStatus = null,
    int? CreatedByUserId = null)
        {
            try
            {
                // Default to active business year if dates are null
                if (startDate == null && endDate == null)
                {
                    var activeYear = await _yearCloseService.GetActiveYearAsync();
                    var bounds = OrderDateRanges.Year(activeYear);
                    startDate = bounds.Start;
                    endDate = bounds.End;
                }

                DateTime start = startDate ?? DateTime.MinValue;
                DateTime end = endDate ?? DateTime.MaxValue;

                // Date-only end (midnight from query) = inclusive through end of that day.
                if (end != DateTime.MaxValue && end.TimeOfDay == TimeSpan.Zero)
                    end = YemenTime.EndOfDay(end);
                if (start != DateTime.MinValue && start.TimeOfDay == TimeSpan.Zero)
                    start = YemenTime.StartOfDay(start);

                // Normalize search term
                searchTerm = searchTerm?.Trim()?.ToLower();

                // Build filter predicate dynamically
                var predicate = PredicateBuilder.New<Order>(true);

                // Apply filters based on provided parameters
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var idMatch = int.TryParse(searchTerm, out var orderId) ? orderId : (int?)null;
                    predicate = predicate.And(order =>
                        (idMatch.HasValue && order.Id == idMatch.Value) ||
                        (order.CustomerName != null && order.CustomerName.ToLower().Contains(searchTerm)) ||
                        (order.DriverName != null && order.DriverName.ToLower().Contains(searchTerm)) ||
                        (order.Customer != null && order.Customer.FullName.ToLower().Contains(searchTerm)) ||
                        (order.Customer != null && order.Customer.PhoneNumber1 != null && order.Customer.PhoneNumber1.Contains(searchTerm)) ||
                        (order.Customer != null && order.Customer.PhoneNumber2 != null && order.Customer.PhoneNumber2.Contains(searchTerm)) ||
                        (order.Driver != null && order.Driver.FullName.ToLower().Contains(searchTerm)) ||
                        order.Title.ToLower().Contains(searchTerm) ||
                        (order.CreatedByUser != null && order.CreatedByUser.FullName.ToLower().Contains(searchTerm)));
                }

                predicate = predicate.And(order => order.DeliveryTime >= start && order.DeliveryTime <= end);

                if (!string.IsNullOrEmpty(SelectedDriver))
                {
                    predicate = predicate.And(order => order.Driver.Id.ToString() == SelectedDriver);
                }

                if (SelectedStatus.HasValue)
                {
                    predicate = predicate.And(order => order.orderStatus == SelectedStatus.Value);
                }

                if (CreatedByUserId.HasValue && CreatedByUserId.Value > 0)
                {
                    predicate = predicate.And(order => order.CreatedByUserId == CreatedByUserId.Value);
                }

                // Query the database using the predicate and include related entities
                var pagedOrders = await _dbContext.GetPagedAsync<Order>(
                    page,
                    pageSize,
                    predicate,
                    new[] { "Driver", "Customer", "CreatedByUser" },
                    true
                );

                // Map to DTOs
                var pagedOrderDTOs = new PagedList<Order>(
                    pagedOrders.TotalCount,
                    pagedOrders.Entities.Select(order => new Order
                    {
                        Id = order.Id,
                        Title = order.Title,
                        FinalPrice = order.FinalPrice,
                        DeliveryTime = order.DeliveryTime,
                        DeliveryFee = order.DeliveryFee,
                        CouponDiscount = order.CouponDiscount,
                        DiscountType = order.DiscountType,
                        DiscountValue = order.DiscountValue,
                        DiscountAmount = order.DiscountAmount,
                        DriverCommissionRate = order.DriverCommissionRate,
                        DriverAmount = order.DriverAmount,
                        CompanyGrossAmount = order.CompanyGrossAmount,
                        CompanyNetAmount = order.CompanyNetAmount,
                        orderStatus = order.orderStatus,
                        PaymentMethod = order.PaymentMethod,
                        Notice=order.Notice,
                        DriverName = !string.IsNullOrWhiteSpace(order.DriverName)
                            ? order.DriverName
                            : (order.Driver?.FullName ?? string.Empty),
                        CustomerName = !string.IsNullOrWhiteSpace(order.CustomerName)
                            ? order.CustomerName
                            : (order.Customer?.FullName ?? string.Empty),
                        CustomerId = order.CustomerId,
                        DriverId = order.DriverId,
                        CreatedByUserId = order.CreatedByUserId,
                        CreatedByUser = order.CreatedByUser == null ? null : new User
                        {
                            Id = order.CreatedByUser.Id,
                            FullName = order.CreatedByUser.FullName,
                            Role = order.CreatedByUser.Role
                        },
                        
                        Customer = order.Customer == null ? null : new Customer
                        {
                            Id = order.Customer.Id,
                            FullName = order.Customer.FullName,
                            PhoneNumber1 = order.Customer.PhoneNumber1,
                            PhoneNumber2 = order.Customer.PhoneNumber2,
                            Address=order.Customer.Address
                        },
                        Driver = order.Driver == null ? null : new Driver
                        {
                            Id = order.Driver.Id,
                            FullName = order.Driver.FullName,
                            PhoneNumber1 = order.Driver.PhoneNumber1,
                            PhoneNumber2 = order.Driver.PhoneNumber2,
                            Address=order.Driver.Address,
                            CommissionRate = order.Driver.CommissionRate
                        }
                    }).ToList(),
                    page,
                    pageSize
                );

                return pagedOrderDTOs;
            }
            catch (Exception ex)
            {
                throw new Exception("خطأ في جلب الطلبات: " + ex.Message);
            }
        }




        public async Task<DriverReportResult> GetDriverReportsAsync(
        int? driverId,
        int page,
        int pageSize,
        string searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? SelectedDriver = null,
        OrderStatus? SelectedStatus = null,
        int? CreatedByUserId = null)
        {
            try
            {
                if (startDate == null && endDate == null)
                {
                    var activeYear = await _yearCloseService.GetActiveYearAsync();
                    var bounds = OrderDateRanges.Year(activeYear);
                    startDate = bounds.Start;
                    endDate = bounds.End;
                }

                DateTime start = startDate ?? DateTime.MinValue;
                DateTime end = endDate ?? DateTime.MaxValue;

                // Date-only end (midnight from picker) = inclusive through end of that day.
                if (end != DateTime.MaxValue && end.TimeOfDay == TimeSpan.Zero)
                    end = YemenTime.EndOfDay(end);
                if (start != DateTime.MinValue && start.TimeOfDay == TimeSpan.Zero)
                    start = YemenTime.StartOfDay(start);

                searchTerm = searchTerm?.Trim()?.ToLower();

                // Prefer explicit SelectedDriver; fall back to driverId
                int? effectiveDriverId = SelectedDriver ?? driverId;

                var predicate = PredicateBuilder.New<Order>(true);

                if (effectiveDriverId.HasValue)
                {
                    predicate = predicate.And(order => order.DriverId == effectiveDriverId.Value);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    predicate = predicate.And(order =>
                        order.Customer.FullName.ToLower().Contains(searchTerm) ||
                        order.Driver.FullName.ToLower().Contains(searchTerm) ||
                        order.Title.ToLower().Contains(searchTerm));
                }

                predicate = predicate.And(order => order.DeliveryTime >= start && order.DeliveryTime <= end);

                if (SelectedStatus.HasValue)
                {
                    predicate = predicate.And(order => order.orderStatus == SelectedStatus.Value);
                }

                if (CreatedByUserId.HasValue && CreatedByUserId.Value > 0)
                {
                    predicate = predicate.And(order => order.CreatedByUserId == CreatedByUserId.Value);
                }

                var pagedOrders = await _dbContext.GetPagedAsync<Order>(
                    page,
                    pageSize,
                    predicate,
                    new[] { "Driver", "Customer", "CreatedByUser" },
                    true
                );

                var query = _dbContext._dbContext.Set<Order>()
                    .Include(order => order.Driver)
                    .Where(order =>
                        order.orderStatus == OrderStatus.Delivered &&
                        order.DeliveryTime >= start &&
                        order.DeliveryTime <= end
                    );

                if (effectiveDriverId.HasValue)
                {
                    query = query.Where(order => order.DriverId == effectiveDriverId.Value);
                }

                if (CreatedByUserId.HasValue && CreatedByUserId.Value > 0)
                {
                    query = query.Where(order => order.CreatedByUserId == CreatedByUserId.Value);
                }

                decimal totalRevenue = await query.SumAsync(order => order.FinalPrice);
                decimal driverProfit = await query.SumAsync(order =>
                    order.DriverAmount > 0
                        ? order.DriverAmount
                        : order.FinalPrice * (
                            order.Driver != null && order.Driver.CommissionRate > 0
                                ? order.Driver.CommissionRate
                                : RevenueSplit.DefaultDriverCommissionRate));

                var fundEntities = await _companyFundService.GetActiveEntitiesAsync();
                var breakdown = RevenueSplit.Calculate(totalRevenue, driverProfit, fundEntities);
                var companyProfit = breakdown.Funds.Count > 0
                    ? breakdown.CompanyAllocatedTotal
                    : breakdown.RemainingAfterDrivers;

                var pagedOrderDTOs = new PagedList<Order>(
                   pagedOrders.TotalCount,
                   pagedOrders.Entities.Select(order => new Order
                   {
                       Id = order.Id,
                       Title = order.Title,
                       FinalPrice = order.FinalPrice,
                       DeliveryTime = order.DeliveryTime,
                       orderStatus = order.orderStatus,
                       PaymentMethod = order.PaymentMethod,
                       Notice = order.Notice,
                       DriverName = !string.IsNullOrWhiteSpace(order.DriverName)
                           ? order.DriverName
                           : (order.Driver?.FullName ?? string.Empty),
                       CustomerName = !string.IsNullOrWhiteSpace(order.CustomerName)
                           ? order.CustomerName
                           : (order.Customer?.FullName ?? string.Empty),
                       CustomerId = order.CustomerId,
                       DriverId = order.DriverId,
                       CreatedByUserId = order.CreatedByUserId,
                       CreatedByUser = order.CreatedByUser == null ? null : new User
                       {
                           Id = order.CreatedByUser.Id,
                           FullName = order.CreatedByUser.FullName
                       },
                       Customer = order.Customer == null ? null : new Customer
                       {
                           Id = order.Customer.Id,
                           FullName = order.Customer.FullName,
                       },
                       Driver = order.Driver == null ? null : new Driver
                       {
                           Id = order.Driver.Id,
                           FullName = order.Driver.FullName,
                           CommissionRate = order.Driver.CommissionRate
                       }
                   }).ToList(),
                   page,
                   pageSize
               );

                return new DriverReportResult
                {
                    Orders = pagedOrderDTOs,
                    DriverProfit = breakdown.DriverProfit,
                    CompanyProfit = companyProfit,
                    CompanyRevenue = breakdown.TotalRevenue,
                    RemainingAfterDrivers = breakdown.RemainingAfterDrivers,
                    Funds = breakdown.Funds,
                    expinses = breakdown.Expenses,
                    ManagersAmount = breakdown.ManagersAmount,
                    ManagementAmount = breakdown.ManagementAmount,
                    CompanyReserveAmount = breakdown.CompanyReserveAmount,
                };
            }
            catch (Exception ex)
            {
                throw new Exception("حدث خطأ أثناء جلب تقارير الموصلين.", ex);
            }
        }

        public async Task<IReadOnlyList<string>> GetDeliveryLocationSuggestionsAsync(string? search = null, int limit = 15)
        {
            limit = Math.Clamp(limit, 1, 30);
            var term = search?.Trim();

            var orders = _dbContext._dbContext.Set<Order>().AsNoTracking();
            var customers = _dbContext._dbContext.Set<Customer>().AsNoTracking();

            var query = from o in orders
                        join c in customers on o.CustomerId equals c.Id
                        where c.Address != null && c.Address.Trim() != ""
                        select c.Address;

            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(a => a.Contains(term));

            return await query
                .GroupBy(a => a.Trim())
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => g.Key)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<string>> GetOrderNoticeSuggestionsAsync(string? search = null, int limit = 15)
        {
            limit = Math.Clamp(limit, 1, 30);
            var term = search?.Trim();

            var query = _dbContext._dbContext.Set<Order>().AsNoTracking()
                .Where(o => o.Notice != null && o.Notice.Trim() != "");

            if (!string.IsNullOrWhiteSpace(term))
                query = query.Where(o => o.Notice.Contains(term));

            return await query
                .GroupBy(o => o.Notice.Trim())
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => g.Key)
                .Take(limit)
                .ToListAsync();
        }


    }
}
