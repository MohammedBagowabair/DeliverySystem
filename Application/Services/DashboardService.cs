using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        public IApplicationDbContext _dbContext;
        public DashboardService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        //Driver Revenue
        public async Task<decimal> CalculateDriverRevenueAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Set default date range
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? DateTime.MaxValue;

            // Calculate revenue directly in the database
            var revenue = await _dbContext._dbContext.Set<Order>()
                .Where(order =>
                    order.DriverId == driverId &&
                    order.orderStatus == OrderStatus.Delivered &&
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end
                    
                    
                    )
                .SumAsync(order => order.FinalPrice * order.Driver.CommissionRate / 100);

            return revenue;
        }


        public async Task<int> GetTotalUsersAsync()
        {
            return await _dbContext.CountAsync<User>();
        }
        public async Task<int> GetTotalDriversAsync()
        {
            return await _dbContext.CountAsync<Driver>();
        }
        public async Task<int> GetTotalCustomersAsync()
        {
            return await _dbContext.CountAsync<Customer>();
        }
        public async Task<int> GetTotalOrdersAsync()
        {
            return await _dbContext.CountAsync<Order>();
        }
    }

}
