using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Driver : Person
    {
        public decimal CommissionRate { get; set; }   // Commission rate (e.g., 70%)
        public string Shift { get; set; }             // Driver shift (e.g., Morning, Night)
    }

}
