using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.OrderDtos
{
    public class DriverPagedDTO:PersonDTO
    {
        public decimal CommissionRate { get; set; }
        public string Shift { get; set; } = string.Empty;
    }
}
