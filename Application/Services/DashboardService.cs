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
        public DashboardService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        //Driver Profit
        public async Task<decimal> CalculateDriverProfitAsync(int? driverId, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Set default date range
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.MaxValue;

            // Calculate revenue directly in the database
            var profit = await _dbContext._dbContext.Set<Order>()
             .Where(order =>
                 (!driverId.HasValue || order.DriverId == driverId) && // If driverId is null, include all drivers
                 order.orderStatus == OrderStatus.Delivered &&
                 order.DeliveryTime >= start &&
                 order.DeliveryTime <= end
             )
         .SumAsync(order => order.FinalPrice * RevenueSplit.DriverShare);

            return profit;

        }

        // Overall Revenue 
        public async Task<decimal> CalculateRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Set default date range
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.MaxValue;

            // Calculate revenue directly in the database
            var revenue = await _dbContext._dbContext.Set<Order>()
                .Where(order =>
                    order.orderStatus == OrderStatus.Delivered &&
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end
                    )
                .SumAsync(order => order.FinalPrice);

            return revenue;
        }

        // Overall company profit (20% of delivered revenue; 10% expenses are separate)
        public async Task<decimal> CalculateProfitAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Set default date range
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.MaxValue;

            var profit = await _dbContext._dbContext.Set<Order>()
               .Where(order =>
                   order.orderStatus == OrderStatus.Delivered &&
                   order.DeliveryTime >= start &&
                   order.DeliveryTime <= end
                   )
               .SumAsync(order => order.FinalPrice * RevenueSplit.CompanyProfitShare);

            return profit;
        }

        public async Task<int> GetTotalUsersAsync()
        {
            return await _dbContext.CountAsync<User>();
        }
        public async Task<int> GetActiveUsersAsync()
        {
            return await _dbContext.CountAsync<User>(x => x.IsActive == true);
        }
        public async Task<int> GetinActiveUsersAsync()
        {
            return await _dbContext.CountAsync<User>(x => x.IsActive == false);
        }




        public async Task<int> GetTotalDriversAsync()
        {
            return await _dbContext.CountAsync<Driver>();
        }
        public async Task<int> GetTotalCustomersAsync()
        {
            return await _dbContext.CountAsync<Customer>();
        }


        // Order Dashboard
        public async Task<int> GetTotalOrdersAsync()
        {
            return await _dbContext.CountAsync<Order>();
        }
        public async Task<int> GetDliveriedOrdersAsync()
        {
            return await _dbContext.CountAsync<Order>(x=>x.orderStatus==OrderStatus.Delivered);
        }
        public async Task<int> GetCancelledOrdersAsync()
        {
            return await _dbContext.CountAsync<Order>(x => x.orderStatus == OrderStatus.Canceled);
        }
        public async Task<int> GetProcessingOrdersAsync()
        {
            return await _dbContext.CountAsync<Order>(x => x.orderStatus == OrderStatus.Processing);
        }





    }

}
