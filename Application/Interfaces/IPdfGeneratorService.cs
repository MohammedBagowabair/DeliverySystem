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
    }
}
