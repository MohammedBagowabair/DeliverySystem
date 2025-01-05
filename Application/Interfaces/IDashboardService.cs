using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDashboardService
    {
        // Driver Revenue
        Task<decimal> CalculateDriverRevenueAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null);


        Task<int> GetTotalUsersAsync();
        Task<int> GetTotalDriversAsync();
        Task<int> GetTotalCustomersAsync();
        Task<int> GetTotalOrdersAsync();


    }
}
