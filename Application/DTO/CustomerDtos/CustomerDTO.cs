using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTO.OrderDtos;

namespace Application.DTO.CustomerDtos
{
    public class CustomerDTO : PersonDTO
    {
        public List<OrderDTO>? Orders { get; set; }  // List of orders as OrderDTOs
    }

}
