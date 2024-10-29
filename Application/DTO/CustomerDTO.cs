using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class CustomerDTO : PersonDTO
    {
        public List<OrderDTO> Orders { get; set; }  // List of orders as OrderDTOs
    }

}
