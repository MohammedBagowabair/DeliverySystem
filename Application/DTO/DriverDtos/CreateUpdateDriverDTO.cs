using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTO.OrderDtos;

namespace Application.DTO.DriverDtos
{
    public class CreateUpdateDriverDTO : PersonDTO
    {
        public decimal CommissionRate { get; set; }
        public string Shift { get; set; } = string.Empty;
        public List<OrderDTO>? Orders { get; set; }
    }
}
