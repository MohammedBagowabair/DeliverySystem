using Application.Helpers;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        public IApplicationDbContext _dbContext;
        private readonly ICompanyFundService _companyFundService;
        private readonly IYearCloseService _yearCloseService;
        private readonly IBusinessDayService _businessDayService;

        public DashboardService(
            IApplicationDbContext dbContext,
            ICompanyFundService companyFundService,
            IYearCloseService yearCloseService,
            IBusinessDayService businessDayService)
        {
            _dbContext = dbContext;
            _companyFundService = companyFundService;
            _yearCloseService = yearCloseService;
            _businessDayService = businessDayService;
        }

        private async Task<(DateTime Start, DateTime End)> ResolveRangeAsync(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue || endDate.HasValue)
            {
                var start = startDate ?? DateTime.MinValue;
                var end = endDate ?? DateTime.MaxValue;
                if (end != DateTime.MaxValue && end.TimeOfDay == TimeSpan.Zero)
                    end = YemenTime.EndOfDay(end);
                if (start != DateTime.MinValue && start.TimeOfDay == TimeSpan.Zero)
                    start = YemenTime.StartOfDay(start);
                return (start, end);
            }

            // No dates = current business day (not whole year).
            var businessDate = await _businessDayService.GetCurrentBusinessDateAsync();
            return OrderDateRanges.Today(businessDate);
        }

        public async Task<decimal> CalculateDriverProfitAsync(int? driverId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var (start, end) = await ResolveRangeAsync(startDate, endDate);

            var profit = await _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Where(order =>
                    (!driverId.HasValue || order.DriverId == driverId) &&
                    order.orderStatus == OrderStatus.Delivered &&
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end)
                .SumAsync(order =>
                    order.DriverAmount > 0
                        ? order.DriverAmount
                        : order.FinalPrice * (
                            order.Driver != null && order.Driver.CommissionRate > 0
                                ? order.Driver.CommissionRate
                                : RevenueSplit.DefaultDriverCommissionRate));

            return profit;
        }

        public async Task<decimal> CalculateRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var (start, end) = await ResolveRangeAsync(startDate, endDate);

            var revenue = await _dbContext._dbContext.Set<Order>()
                .Where(order =>
                    order.orderStatus == OrderStatus.Delivered &&
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end)
                .SumAsync(order => order.FinalPrice);

            return revenue;
        }

        public async Task<decimal> CalculateProfitAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var (start, end) = await ResolveRangeAsync(startDate, endDate);

            var delivered = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Where(order =>
                    order.orderStatus == OrderStatus.Delivered &&
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end);

            var totalRevenue = await delivered.SumAsync(order => order.FinalPrice);
            var driverProfit = await delivered.SumAsync(order =>
                order.DriverAmount > 0
                    ? order.DriverAmount
                    : order.FinalPrice * (
                        order.Driver != null && order.Driver.CommissionRate > 0
                            ? order.Driver.CommissionRate
                            : RevenueSplit.DefaultDriverCommissionRate));
            var fundEntities = await _companyFundService.GetActiveEntitiesAsync();
            var breakdown = RevenueSplit.Calculate(totalRevenue, driverProfit, fundEntities);
            return breakdown.Funds.Count > 0
                ? breakdown.CompanyAllocatedTotal
                : breakdown.RemainingAfterDrivers;
        }

        public async Task<int> GetTotalUsersAsync()
            => await _dbContext.CountAsync<User>();

        public async Task<int> GetActiveUsersAsync()
            => await _dbContext.CountAsync<User>(x => x.IsActive == true);

        public async Task<int> GetinActiveUsersAsync()
            => await _dbContext.CountAsync<User>(x => x.IsActive == false);

        public async Task<int> GetTotalDriversAsync()
            => await _dbContext.CountAsync<Driver>();

        public async Task<int> GetTotalCustomersAsync()
            => await _dbContext.CountAsync<Customer>();

        public async Task<int> GetTotalOrdersAsync()
        {
            var (start, end) = await ResolveRangeAsync(null, null);
            return await _dbContext.CountAsync<Order>(o => o.DeliveryTime >= start && o.DeliveryTime <= end);
        }

        public async Task<int> GetDliveriedOrdersAsync()
        {
            var (start, end) = await ResolveRangeAsync(null, null);
            return await _dbContext.CountAsync<Order>(x =>
                x.orderStatus == OrderStatus.Delivered &&
                x.DeliveryTime >= start &&
                x.DeliveryTime <= end);
        }

        public async Task<int> GetCancelledOrdersAsync()
        {
            var (start, end) = await ResolveRangeAsync(null, null);
            return await _dbContext.CountAsync<Order>(x =>
                x.orderStatus == OrderStatus.Canceled &&
                x.DeliveryTime >= start &&
                x.DeliveryTime <= end);
        }

        public async Task<int> GetProcessingOrdersAsync()
        {
            var (start, end) = await ResolveRangeAsync(null, null);
            return await _dbContext.CountAsync<Order>(x =>
                x.orderStatus == OrderStatus.Processing &&
                x.DeliveryTime >= start &&
                x.DeliveryTime <= end);
        }
    }
}
