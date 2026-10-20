using Application.DTO.DayCloseDtos;
using Application.Helpers;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class DayCloseService : IDayCloseService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IPdfGeneratorService _pdfGeneratorService;
        private readonly IYearCloseService _yearCloseService;

        public DayCloseService(
            IApplicationDbContext dbContext,
            IPdfGeneratorService pdfGeneratorService,
            IYearCloseService yearCloseService)
        {
            _dbContext = dbContext;
            _pdfGeneratorService = pdfGeneratorService;
            _yearCloseService = yearCloseService;
        }

        public async Task<bool> IsClosedAsync(DateTime businessDate)
        {
            var day = businessDate.Date;
            return await _dbContext._dbContext.Set<ClosedBusinessDay>()
                .AsNoTracking()
                .AnyAsync(x => x.BusinessDate == day);
        }

        public async Task<DayCloseEligibilityDTO> GetEligibilityAsync(DateTime businessDate, bool allowOpenBusinessDay = false)
        {
            var day = businessDate.Date;
            var start = YemenTime.StartOfDay(day);
            var end = YemenTime.EndOfDay(day);

            var orders = await _dbContext._dbContext.Set<Order>()
                .AsNoTracking()
                .Where(o => o.DeliveryTime >= start && o.DeliveryTime <= end)
                .Select(o => o.orderStatus)
                .ToListAsync();

            var processing = orders.Count(s => s == OrderStatus.Processing);
            var delivered = orders.Count(s => s == OrderStatus.Delivered);
            var canceled = orders.Count(s => s == OrderStatus.Canceled);
            var isClosed = await IsClosedAsync(day);

            string? block = null;
            var canClose = true;

            if (isClosed)
            {
                canClose = false;
                block = "هذا اليوم مغلق مسبقاً.";
            }
            else
            {
                var currentBusiness = await _dbContext._dbContext.Set<BusinessDayState>()
                    .AsNoTracking()
                    .Where(x => x.Id == 1)
                    .Select(x => (DateTime?)x.CurrentBusinessDate)
                    .FirstOrDefaultAsync();

                var workingBusinessDate = (currentBusiness ?? YemenTime.Today).Date;

                if (day > workingBusinessDate)
                {
                    canClose = false;
                    block = "لا يمكن إغلاق يوم بعد يوم العمل المفتوح الحالي.";
                }
                else if (day == workingBusinessDate && !allowOpenBusinessDay)
                {
                    canClose = false;
                    block = workingBusinessDate < YemenTime.Today
                        ? "يوم العمل الحالي متأخر عن التقويم — استخدم «إغلاق وأرشفة يوم العمل وبدء اليوم» أعلاه (وليس الإغلاق اليدوي)."
                        : "يوم العمل الحالي ما زال اليوم — يبقى مفتوحاً. يُغلق ويُؤرشف عند الانتقال لليوم التالي بعد تغيير التقويم. الإغلاق اليدوي متاح للأيام السابقة فقط.";
                }
                else if (await _yearCloseService.IsClosedAsync(day.Year))
                {
                    canClose = false;
                    block = "السنة مغلقة — لا يمكن إغلاق أيام إضافية فيها.";
                }
                else if (processing > 0)
                {
                    canClose = false;
                    block = $"يوجد {processing} طلب قيد المعالجة. يجب تسليمها أو رفضها قبل إغلاق اليوم.";
                }
            }

            return new DayCloseEligibilityDTO
            {
                BusinessDate = day,
                IsClosed = isClosed,
                CanClose = canClose,
                ProcessingCount = processing,
                DeliveredCount = delivered,
                CanceledCount = canceled,
                TotalOrders = orders.Count,
                BlockReason = block
            };
        }

        public async Task<List<ClosedBusinessDayDTO>> GetClosedDaysAsync(int take = 60, int? year = null)
        {
            if (take < 1) take = 30;
            if (take > 365) take = 365;

            var query = _dbContext._dbContext.Set<ClosedBusinessDay>()
                .AsNoTracking()
                .Include(x => x.ClosedByUser)
                .AsQueryable();

            if (year.HasValue)
            {
                var (start, end) = OrderDateRanges.Year(year.Value);
                query = query.Where(x => x.BusinessDate >= start.Date && x.BusinessDate <= end.Date);
            }

            return await query
                .OrderByDescending(x => x.BusinessDate)
                .Take(take)
                .Select(x => Map(x))
                .ToListAsync();
        }

        public async Task<DayArchiveDetailDTO> GetArchiveDetailAsync(DateTime businessDate)
        {
            var day = businessDate.Date;
            var closed = await _dbContext._dbContext.Set<ClosedBusinessDay>()
                .AsNoTracking()
                .Include(x => x.ClosedByUser)
                .FirstOrDefaultAsync(x => x.BusinessDate == day)
                ?? throw new DeliveryCoreException(
                    "هذا اليوم غير مؤرشف / غير مغلق.",
                    ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE,
                    null);

            var start = YemenTime.StartOfDay(day);
            var end = YemenTime.EndOfDay(day);

            var drivers = await _dbContext._dbContext.Set<Order>()
                .AsNoTracking()
                .Include(o => o.Driver)
                .Where(o =>
                    o.DeliveryTime >= start &&
                    o.DeliveryTime <= end &&
                    o.orderStatus == OrderStatus.Delivered &&
                    o.DriverId > 0)
                .GroupBy(o => new
                {
                    o.DriverId,
                    Name = !string.IsNullOrWhiteSpace(o.DriverName)
                        ? o.DriverName
                        : (o.Driver != null ? o.Driver.FullName : ("#" + o.DriverId))
                })
                .Select(g => new DayArchiveDriverSummaryDTO
                {
                    DriverId = g.Key.DriverId,
                    DriverName = g.Key.Name,
                    DeliveredCount = g.Count(),
                    Revenue = g.Sum(x => x.FinalPrice)
                })
                .OrderBy(x => x.DriverName)
                .ToListAsync();

            return new DayArchiveDetailDTO
            {
                Day = Map(closed),
                Drivers = drivers
            };
        }

        public async Task<ClosedBusinessDayDTO> CloseDayAsync(
            DateTime businessDate,
            int closedByUserId,
            string? notes,
            bool allowOpenBusinessDay = false)
        {
            if (closedByUserId <= 0)
            {
                throw new DeliveryCoreException(
                    "تعذر تحديد المستخدم الذي يغلق اليوم.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var eligibility = await GetEligibilityAsync(businessDate, allowOpenBusinessDay);
            if (!eligibility.CanClose)
            {
                throw new DeliveryCoreException(
                    eligibility.BlockReason ?? "لا يمكن إغلاق هذا اليوم.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var day = businessDate.Date;
            var start = YemenTime.StartOfDay(day);
            var end = YemenTime.EndOfDay(day);

            var dayOrders = await _dbContext._dbContext.Set<Order>()
                .AsNoTracking()
                .Where(o => o.DeliveryTime >= start && o.DeliveryTime <= end)
                .ToListAsync();

            var entity = new ClosedBusinessDay
            {
                BusinessDate = day,
                ClosedAt = YemenTime.Now,
                ClosedByUserId = closedByUserId,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                TotalOrders = dayOrders.Count,
                DeliveredOrders = dayOrders.Count(o => o.orderStatus == OrderStatus.Delivered),
                CanceledOrders = dayOrders.Count(o => o.orderStatus == OrderStatus.Canceled),
                TotalRevenue = dayOrders
                    .Where(o => o.orderStatus == OrderStatus.Delivered)
                    .Sum(o => o.FinalPrice)
            };

            await _dbContext._dbContext.Set<ClosedBusinessDay>().AddAsync(entity);
            await _dbContext._dbContext.SaveChangesAsync();

            var withUser = await _dbContext._dbContext.Set<ClosedBusinessDay>()
                .AsNoTracking()
                .Include(x => x.ClosedByUser)
                .FirstAsync(x => x.Id == entity.Id);

            return Map(withUser);
        }

        public async Task RefreshClosedDayTotalsAsync(DateTime businessDate)
        {
            var day = businessDate.Date;
            var closed = await _dbContext._dbContext.Set<ClosedBusinessDay>()
                .FirstOrDefaultAsync(x => x.BusinessDate == day);
            if (closed is null)
                return;

            var start = YemenTime.StartOfDay(day);
            var end = YemenTime.EndOfDay(day);
            var dayOrders = await _dbContext._dbContext.Set<Order>()
                .AsNoTracking()
                .Where(o => o.DeliveryTime >= start && o.DeliveryTime <= end)
                .ToListAsync();

            closed.TotalOrders = dayOrders.Count;
            closed.DeliveredOrders = dayOrders.Count(o => o.orderStatus == OrderStatus.Delivered);
            closed.CanceledOrders = dayOrders.Count(o => o.orderStatus == OrderStatus.Canceled);
            closed.TotalRevenue = dayOrders
                .Where(o => o.orderStatus == OrderStatus.Delivered)
                .Sum(o => o.FinalPrice);

            await _dbContext._dbContext.SaveChangesAsync();
        }

        public async Task<byte[]> GenerateAllOrdersPdfAsync(DateTime businessDate)
        {
            var day = await RequireClosedDayAsync(businessDate);
            var orders = await LoadDayOrdersAsync(day.BusinessDate, driverId: null);
            var title = $"أرشيف يومي — جميع الطلبات ({day.BusinessDate:dd/MM/yyyy})";
            return _pdfGeneratorService.GenerateOrderPdf(orders, title, "جميع الموصلين");
        }

        public async Task<byte[]> GenerateDriverPdfAsync(DateTime businessDate, int driverId)
        {
            var day = await RequireClosedDayAsync(businessDate);
            if (driverId <= 0)
            {
                throw new DeliveryCoreException(
                    "معرف الموصل غير صالح.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var orders = await LoadDayOrdersAsync(day.BusinessDate, driverId);
            var driverName = orders.Select(o =>
                    !string.IsNullOrWhiteSpace(o.DriverName) ? o.DriverName : o.Driver?.FullName)
                .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                ?? await _dbContext._dbContext.Set<Driver>()
                    .AsNoTracking()
                    .Where(d => d.Id == driverId)
                    .Select(d => d.FullName)
                    .FirstOrDefaultAsync()
                ?? $"موصل #{driverId}";

            var title = $"أرشيف يومي — {driverName} ({day.BusinessDate:dd/MM/yyyy})";
            return _pdfGeneratorService.GenerateOrderPdf(orders, title, driverName);
        }

        private async Task<ClosedBusinessDay> RequireClosedDayAsync(DateTime businessDate)
        {
            var day = businessDate.Date;
            return await _dbContext._dbContext.Set<ClosedBusinessDay>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BusinessDate == day)
                ?? throw new DeliveryCoreException(
                    "لا يمكن إنشاء أرشيف ليوم غير مغلق.",
                    ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE,
                    null);
        }

        private async Task<List<Order>> LoadDayOrdersAsync(DateTime businessDate, int? driverId)
        {
            var start = YemenTime.StartOfDay(businessDate);
            var end = YemenTime.EndOfDay(businessDate);

            var query = _dbContext._dbContext.Set<Order>()
                .AsNoTracking()
                .Include(o => o.Driver)
                .Include(o => o.Customer)
                .Include(o => o.CreatedByUser)
                .Where(o => o.DeliveryTime >= start && o.DeliveryTime <= end);

            if (driverId.HasValue)
                query = query.Where(o => o.DriverId == driverId.Value);

            return await query
                .OrderBy(o => o.DeliveryTime)
                .ThenBy(o => o.Id)
                .ToListAsync();
        }

        private static ClosedBusinessDayDTO Map(ClosedBusinessDay x) => new()
        {
            Id = x.Id,
            BusinessDate = x.BusinessDate,
            ClosedAt = x.ClosedAt,
            ClosedByUserId = x.ClosedByUserId,
            ClosedByUserName = x.ClosedByUser?.FullName,
            Notes = x.Notes,
            TotalOrders = x.TotalOrders,
            DeliveredOrders = x.DeliveredOrders,
            CanceledOrders = x.CanceledOrders,
            TotalRevenue = x.TotalRevenue
        };
    }
}
