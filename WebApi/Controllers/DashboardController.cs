using Application.Common.Models;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {

        public IApplicationDbContext _dbContext;


        private readonly IDashboardService _service;
        private readonly IMapper _mapper;
        public DashboardController(IDashboardService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }


        // Driver Revenue
        [HttpGet("DriversProfit")]
        public async Task<ApiResultModel<decimal>> GetDriverProfit(int? driverId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var profit = await _service.CalculateDriverProfitAsync(driverId, startDate, endDate);
                return new ApiResultModel<decimal>(profit);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<decimal>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<decimal>(500, ex.Message, 0);
            }
        }
        // Profit

        [HttpGet("GetProfit")]
        public async Task<ApiResultModel<decimal>> GetProfit([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var profit = await _service.CalculateProfitAsync(startDate, endDate);
                return new ApiResultModel<decimal>(profit);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<decimal>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<decimal>(500, ex.Message, 0);
            }
        }

        // Overall Revenue
        [HttpGet("GetRevenue")]
        public async Task<ApiResultModel<decimal>> GetRevenue([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var revenue = await _service.CalculateRevenueAsync(startDate, endDate);
                return new ApiResultModel<decimal>(revenue);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<decimal>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<decimal>(500, ex.Message, 0);
            }
        }

        // TotalUsers
        [HttpGet("TotalUsers")]
        public async Task<ApiResultModel<int>> GetTotalUsers()
        {

            try
            {
                var totalUsers = await _service.GetTotalUsersAsync();
                return new ApiResultModel<int>(totalUsers);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<int>(500, ex.Message, 0);
            }

        }

        [HttpGet("TotalActiveUsers")]
        public async Task<ApiResultModel<int>> GetTotalActiveUsers()
        {

            try
            {
                var totalActiveUsers = await _service.GetActiveUsersAsync();
                return new ApiResultModel<int>(totalActiveUsers);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<int>(500, ex.Message, 0);
            }

        }
        [HttpGet("TotalinActiveUsers")]
        public async Task<ApiResultModel<int>> GetTotalinActiveUsers()
        {

            try
            {
                var totalinActiveUsers = await _service.GetinActiveUsersAsync();
                return new ApiResultModel<int>(totalinActiveUsers);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<int>(500, ex.Message, 0);
            }

        }

        // TotalDrivers
        [HttpGet("TotalDrivers")]
        public async Task<ApiResultModel<int>> GetTotalDrivers()
        {

            try
            {
                var totalDrivers = await _service.GetTotalDriversAsync();
                return new ApiResultModel<int>(totalDrivers);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<int>(500, ex.Message, 0);
            }
        }
        // TotalCustomers
        [HttpGet("TotalCustomers")]
        public async Task<ApiResultModel<int>> GetTotalCustomers()
        {
            try
            {
                var totalCustomers = await _service.GetTotalCustomersAsync();
                return new ApiResultModel<int>(totalCustomers);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<int>(500, ex.Message, 0);
            }
        }

        // TotalOrders
        [HttpGet("TotalOrders")]
        public async Task<ApiResultModel<int>> GetTotalOrders()
        {
            try
            {
                var totalOrders = await _service.GetTotalOrdersAsync();
                return new ApiResultModel<int>(totalOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<int>(500, ex.Message, 0);
            }
        }

        // Dliveried Orders
        [HttpGet("DliveriedOrders")]
        public async Task<ApiResultModel<int>> GetDliveriedOrders()
        {
            try
            {
                var totalOrders = await _service.GetDliveriedOrdersAsync();
                return new ApiResultModel<int>(totalOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<int>(500, ex.Message, 0);
            }
        }
        // Processing Orders
        [HttpGet("ProcessingOrders")]
        public async Task<ApiResultModel<int>> GetProcessingOrders()
        {
            try
            {
                var totalOrders = await _service.GetProcessingOrdersAsync();
                return new ApiResultModel<int>(totalOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<int>(500, ex.Message, 0);
            }
        }
        // CancelledOrders Orders
        [HttpGet("CancelledOrders")]
        public async Task<ApiResultModel<int>> GetCancelledOrders()
        {
            try
            {
                var totalOrders = await _service.GetCancelledOrdersAsync();
                return new ApiResultModel<int>(totalOrders);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<int>(ex.Code, ex.Message, 0);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<int>(500, ex.Message, 0);
            }
        }

    }
}
