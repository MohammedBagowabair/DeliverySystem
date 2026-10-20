using Application.DTO.CompanyFundDtos;
using Application.DTO.YearCloseDtos;
using Application.Helpers;
using Application.Interfaces;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Application.Services
{
    public class YearCloseService : IYearCloseService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly IApplicationDbContext _dbContext;
        private readonly IPdfGeneratorService _pdfGeneratorService;
        private readonly ICompanyFundService _companyFundService;

        public YearCloseService(
            IApplicationDbContext dbContext,
            IPdfGeneratorService pdfGeneratorService,
            ICompanyFundService companyFundService)
        {
            _dbContext = dbContext;
            _pdfGeneratorService = pdfGeneratorService;
            _companyFundService = companyFundService;
        }

        public async Task<int> GetActiveYearAsync()
        {
            var closed = await _dbContext._dbContext.Set<ClosedBusinessYear>()
                .AsNoTracking()
                .Select(x => x.Year)
                .ToListAsync();

            var year = YemenTime.Today.Year;
            while (closed.Contains(year))
                year++;

            return year;
        }

        public async Task<ActiveYearDTO> GetActiveYearInfoAsync()
        {
            var year = await GetActiveYearAsync();
            var bounds = OrderDateRanges.Year(year);
            return new ActiveYearDTO
            {
                ActiveYear = year,
                YearStart = bounds.Start,
                YearEnd = bounds.End,
                YemenToday = YemenTime.Today
            };
        }

        public async Task<bool> IsClosedAsync(int year)
        {
            return await _dbContext._dbContext.Set<ClosedBusinessYear>()
                .AsNoTracking()
                .AnyAsync(x => x.Year == year);
        }

        public async Task<YearCloseEligibilityDTO> GetEligibilityAsync(int year)
        {
            var (start, end) = OrderDateRanges.Year(year);
            var orders = await _dbContext._dbContext.Set<Order>()
                .AsNoTracking()
                .Where(o => o.DeliveryTime >= start && o.DeliveryTime <= end)
                .Select(o => o.orderStatus)
                .ToListAsync();

            var processing = orders.Count(s => s == OrderStatus.Processing);
            var delivered = orders.Count(s => s == OrderStatus.Delivered);
            var canceled = orders.Count(s => s == OrderStatus.Canceled);
            var isClosed = await IsClosedAsync(year);

            string? block = null;
            var canClose = true;

            if (isClosed)
            {
                canClose = false;
                block = "هذه السنة مغلقة مسبقاً.";
            }
            else if (year >= YemenTime.Today.Year)
            {
                canClose = false;
                block = year > YemenTime.Today.Year
                    ? "لا يمكن إغلاق سنة مستقبلية."
                    : "يمكن إغلاق السنة فقط بعد بداية السنة التالية (بتوقيت اليمن).";
            }
            else if (processing > 0)
            {
                canClose = false;
                block = $"يوجد {processing} طلب قيد المعالجة في سنة {year}. يجب تسليمها أو رفضها قبل إغلاق السنة.";
            }
            else
            {
                var currentBusiness = await _dbContext._dbContext.Set<BusinessDayState>()
                    .AsNoTracking()
                    .Where(x => x.Id == 1)
                    .Select(x => (DateTime?)x.CurrentBusinessDate)
                    .FirstOrDefaultAsync();

                var workingYear = (currentBusiness ?? YemenTime.Today).Year;
                if (workingYear <= year)
                {
                    canClose = false;
                    block = "يجب إغلاق آخر يوم عمل في السنة ونقل مؤشر يوم العمل إلى السنة التالية قبل إغلاق السنة.";
                }
            }

            return new YearCloseEligibilityDTO
            {
                Year = year,
                IsClosed = isClosed,
                CanClose = canClose,
                ProcessingCount = processing,
                DeliveredCount = delivered,
                CanceledCount = canceled,
                TotalOrders = orders.Count,
                BlockReason = block
            };
        }

        public async Task<List<ClosedBusinessYearDTO>> GetClosedYearsAsync()
        {
            var rows = await _dbContext._dbContext.Set<ClosedBusinessYear>()
                .AsNoTracking()
                .Include(x => x.ClosedByUser)
                .OrderByDescending(x => x.Year)
                .ToListAsync();

            return rows.Select(Map).ToList();
        }

        public async Task<YearArchiveDetailDTO> GetArchiveDetailAsync(int year)
        {
            var closed = await _dbContext._dbContext.Set<ClosedBusinessYear>()
                .AsNoTracking()
                .Include(x => x.ClosedByUser)
                .FirstOrDefaultAsync(x => x.Year == year);

            var (start, end) = OrderDateRanges.Year(year);

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
                .Select(g => new YearArchiveDriverSummaryDTO
                {
                    DriverId = g.Key.DriverId,
                    DriverName = g.Key.Name,
                    DeliveredCount = g.Count(),
                    Revenue = g.Sum(x => x.FinalPrice)
                })
                .OrderBy(x => x.DriverName)
                .ToListAsync();

            if (closed != null)
            {
                return new YearArchiveDetailDTO
                {
                    Year = Map(closed),
                    Drivers = drivers,
                    IsClosed = true
                };
            }

            // Open year: live totals for browse (Admin eligibility / preview)
            var eligibility = await GetEligibilityAsync(year);
            return new YearArchiveDetailDTO
            {
                IsClosed = false,
                Year = new ClosedBusinessYearDTO
                {
                    Year = year,
                    TotalOrders = eligibility.TotalOrders,
                    DeliveredOrders = eligibility.DeliveredCount,
                    CanceledOrders = eligibility.CanceledCount,
                    TotalRevenue = await _dbContext._dbContext.Set<Order>()
                        .AsNoTracking()
                        .Where(o =>
                            o.DeliveryTime >= start &&
                            o.DeliveryTime <= end &&
                            o.orderStatus == OrderStatus.Delivered)
                        .SumAsync(o => (decimal?)o.FinalPrice) ?? 0
                },
                Drivers = drivers
            };
        }

        public async Task<ClosedBusinessYearDTO> CloseYearAsync(int year, int closedByUserId, string? notes)
        {
            if (closedByUserId <= 0)
            {
                throw new DeliveryCoreException(
                    "تعذر تحديد المستخدم الذي يغلق السنة.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var eligibility = await GetEligibilityAsync(year);
            if (!eligibility.CanClose)
            {
                throw new DeliveryCoreException(
                    eligibility.BlockReason ?? "لا يمكن إغلاق هذه السنة.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var (start, end) = OrderDateRanges.Year(year);
            var yearOrders = await _dbContext._dbContext.Set<Order>()
                .AsNoTracking()
                .Include(o => o.Driver)
                .Where(o => o.DeliveryTime >= start && o.DeliveryTime <= end)
                .ToListAsync();

            var delivered = yearOrders.Where(o => o.orderStatus == OrderStatus.Delivered).ToList();
            var totalRevenue = delivered.Sum(o => o.FinalPrice);
            var driverProfit = delivered.Sum(o =>
                OrderRevenueCalculator.ResolveDriverProfit(
                    o.DriverAmount, o.FinalPrice, o.Driver?.CommissionRate ?? o.DriverCommissionRate));

            var funds = await _companyFundService.GetActiveEntitiesAsync();
            var fundShares = funds
                .OrderBy(f => f.SortOrder)
                .ThenBy(f => f.Id)
                .Select(f => new FundShareSnapshotDTO
                {
                    FundId = f.Id,
                    Name = f.Name,
                    Share = f.Share
                })
                .ToList();

            var entity = new ClosedBusinessYear
            {
                Year = year,
                ClosedAt = YemenTime.Now,
                ClosedByUserId = closedByUserId,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                TotalOrders = yearOrders.Count,
                DeliveredOrders = delivered.Count,
                CanceledOrders = yearOrders.Count(o => o.orderStatus == OrderStatus.Canceled),
                TotalRevenue = totalRevenue,
                DriverProfit = driverProfit,
                FundSharesJson = JsonSerializer.Serialize(fundShares, JsonOptions)
            };

            await _dbContext._dbContext.Set<ClosedBusinessYear>().AddAsync(entity);
            await _dbContext._dbContext.SaveChangesAsync();

            var withUser = await _dbContext._dbContext.Set<ClosedBusinessYear>()
                .AsNoTracking()
                .Include(x => x.ClosedByUser)
                .FirstAsync(x => x.Id == entity.Id);

            return Map(withUser);
        }

        public async Task<YearProfitDTO> GetYearProfitAsync(int year)
        {
            var (start, end) = OrderDateRanges.Year(year);
            var delivered = await _dbContext._dbContext.Set<Order>()
                .AsNoTracking()
                .Include(o => o.Driver)
                .Where(o =>
                    o.DeliveryTime >= start &&
                    o.DeliveryTime <= end &&
                    o.orderStatus == OrderStatus.Delivered)
                .ToListAsync();

            var totalRevenue = delivered.Sum(o => o.FinalPrice);
            var driverProfit = delivered.Sum(o =>
                OrderRevenueCalculator.ResolveDriverProfit(
                    o.DriverAmount, o.FinalPrice, o.Driver?.CommissionRate ?? o.DriverCommissionRate));

            var closed = await _dbContext._dbContext.Set<ClosedBusinessYear>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Year == year);

            List<CompanyFund> funds;
            var usedSnapshot = false;

            if (closed != null)
            {
                var snapshots = DeserializeFunds(closed.FundSharesJson);
                if (snapshots.Count > 0)
                {
                    usedSnapshot = true;
                    funds = snapshots.Select((s, i) => new CompanyFund
                    {
                        Id = s.FundId > 0 ? s.FundId : i + 1,
                        Name = s.Name,
                        Share = s.Share,
                        SortOrder = i,
                        IsActive = true
                    }).ToList();
                }
                else
                {
                    funds = await _companyFundService.GetActiveEntitiesAsync();
                }
            }
            else
            {
                funds = await _companyFundService.GetActiveEntitiesAsync();
            }

            var breakdown = RevenueSplit.Calculate(totalRevenue, driverProfit, funds);
            return new YearProfitDTO
            {
                Year = year,
                UsedSnapshotFunds = usedSnapshot,
                TotalRevenue = breakdown.TotalRevenue,
                DriverProfit = breakdown.DriverProfit,
                RemainingAfterDrivers = breakdown.RemainingAfterDrivers,
                CompanyAllocatedTotal = breakdown.CompanyAllocatedTotal,
                Funds = breakdown.Funds
            };
        }

        public async Task<PagedList<Order>> GetYearOrdersAsync(int year, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var (start, end) = OrderDateRanges.Year(year);
            var query = _dbContext._dbContext.Set<Order>()
                .AsNoTracking()
                .Include(o => o.Driver)
                .Include(o => o.Customer)
                .Include(o => o.CreatedByUser)
                .Where(o => o.DeliveryTime >= start && o.DeliveryTime <= end);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(o => o.DeliveryTime)
                .ThenByDescending(o => o.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<Order>(total, items, page, pageSize);
        }

        public async Task<byte[]> GenerateAllOrdersPdfAsync(int year)
        {
            await RequireClosedOrOpenBrowseAsync(year);
            var orders = await LoadYearOrdersAsync(year, driverId: null);
            var title = $"أرشيف سنوي — جميع الطلبات ({year})";
            return _pdfGeneratorService.GenerateOrderPdf(orders, title, "جميع الموصلين");
        }

        public async Task<byte[]> GenerateDriverPdfAsync(int year, int driverId)
        {
            await RequireClosedOrOpenBrowseAsync(year);
            if (driverId <= 0)
            {
                throw new DeliveryCoreException(
                    "معرف الموصل غير صالح.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var orders = await LoadYearOrdersAsync(year, driverId);
            var driverName = orders.Select(o =>
                    !string.IsNullOrWhiteSpace(o.DriverName) ? o.DriverName : o.Driver?.FullName)
                .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                ?? await _dbContext._dbContext.Set<Driver>()
                    .AsNoTracking()
                    .Where(d => d.Id == driverId)
                    .Select(d => d.FullName)
                    .FirstOrDefaultAsync()
                ?? $"موصل #{driverId}";

            var title = $"أرشيف سنوي — {driverName} ({year})";
            return _pdfGeneratorService.GenerateOrderPdf(orders, title, driverName);
        }

        /// <summary>PDF allowed for closed years; also for open years for preview (Staff/Admin).</summary>
        private async Task RequireClosedOrOpenBrowseAsync(int year)
        {
            if (year < 2000 || year > YemenTime.Today.Year + 1)
            {
                throw new DeliveryCoreException(
                    "سنة غير صالحة.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            // Closed years always OK; open years OK for browse/PDF of that calendar year.
            _ = await IsClosedAsync(year);
        }

        private async Task<List<Order>> LoadYearOrdersAsync(int year, int? driverId)
        {
            var (start, end) = OrderDateRanges.Year(year);
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

        private static List<FundShareSnapshotDTO> DeserializeFunds(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<FundShareSnapshotDTO>();

            try
            {
                return JsonSerializer.Deserialize<List<FundShareSnapshotDTO>>(json, JsonOptions)
                       ?? new List<FundShareSnapshotDTO>();
            }
            catch
            {
                return new List<FundShareSnapshotDTO>();
            }
        }

        private static ClosedBusinessYearDTO Map(ClosedBusinessYear x) => new()
        {
            Id = x.Id,
            Year = x.Year,
            ClosedAt = x.ClosedAt,
            ClosedByUserId = x.ClosedByUserId,
            ClosedByUserName = x.ClosedByUser?.FullName,
            Notes = x.Notes,
            TotalOrders = x.TotalOrders,
            DeliveredOrders = x.DeliveredOrders,
            CanceledOrders = x.CanceledOrders,
            TotalRevenue = x.TotalRevenue,
            DriverProfit = x.DriverProfit,
            FundShares = DeserializeFunds(x.FundSharesJson)
        };
    }
}
