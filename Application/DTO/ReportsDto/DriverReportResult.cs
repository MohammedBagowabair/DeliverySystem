using Domain.Common.Models;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.ReportsDto
{
    public class DriverReportResult
    {
        public PagedList<Order>? Orders { get; set; }
        public decimal DriverProfit { get; set; }
        public decimal CompanyRevenue { get; set; }
        public decimal CompanyProfit { get; set; }
        public decimal expinses { get; set; }
    }
}
