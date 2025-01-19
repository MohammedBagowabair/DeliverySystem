using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDashboardService
    {
        // Driver Profit
        Task<decimal> CalculateDriverProfitAsync(int? driverId, DateTime? startDate = null, DateTime? endDate = null);

        // Overall Revenue 
        Task<decimal> CalculateRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        // Overall Profit 
        Task<decimal> CalculateProfitAsync(DateTime? startDate = null, DateTime? endDate = null);

        // User Dashboard
        Task<int> GetTotalUsersAsync();
        Task<int> GetActiveUsersAsync();
        Task<int> GetinActiveUsersAsync();


        Task<int> GetTotalDriversAsync();
        Task<int> GetTotalCustomersAsync();


        // Order Dashboard

        Task<int> GetTotalOrdersAsync();
        Task<int> GetDliveriedOrdersAsync();
        Task<int> GetCancelledOrdersAsync();
        Task<int> GetProcessingOrdersAsync();






    }
}
