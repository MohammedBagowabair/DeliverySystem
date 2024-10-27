using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Customer : Person
    {
        public List<Order> Orders { get; set; }       // List of orders placed by the customer
    }

}
