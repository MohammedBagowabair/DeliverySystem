using Application.Common.Models;
using Application.DTO.DriverDtos;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient.DataClassification;
using Microsoft.EntityFrameworkCore;

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
        [HttpGet("{driverId}/DriverRevenue")]
        public async Task<ApiResultModel<decimal>> GetDriverProfit(int driverId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var profit = await _service.CalculateDriverRevenueAsync(driverId, startDate, endDate);
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
    }
}
