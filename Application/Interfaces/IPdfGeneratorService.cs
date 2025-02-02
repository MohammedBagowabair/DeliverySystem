using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPdfGeneratorService
    {
        byte[] GenerateOrderPdf(List<Order> orders,string reportMessage);
        byte[] GenerateDriversPdf(List<Order> orders, string reportMessage, string driverName, decimal driverProfit, decimal companyRevenue, decimal companyProfit,DateTime? startDate= null, DateTime? endDate = null);
    }
}
