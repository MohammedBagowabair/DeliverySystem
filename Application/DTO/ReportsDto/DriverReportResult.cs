using Application.DTO.CompanyFundDtos;
using Domain.Common.Models;
using Domain.Entities;

namespace Application.DTO.ReportsDto
{
    public class DriverReportResult
    {
        public PagedList<Order>? Orders { get; set; }
        public decimal DriverProfit { get; set; }
        public decimal CompanyRevenue { get; set; }
        public decimal CompanyProfit { get; set; }
        public decimal RemainingAfterDrivers { get; set; }
        public List<FundAmountDTO> Funds { get; set; } = new();

        // Legacy fields (filled from fund name matching for older clients).
        public decimal expinses { get; set; }
        public decimal ManagersAmount { get; set; }
        public decimal ManagementAmount { get; set; }
        public decimal CompanyReserveAmount { get; set; }
    }
}
