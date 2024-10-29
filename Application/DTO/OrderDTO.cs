using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class OrderDTO : BaseEntityDTO
    {
        public int CustomerId { get; set; }
        public int DriverId { get; set; }
        public DateTime DeliveryTime { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal DeliveryFee { get; set; }
        public decimal CouponDiscount { get; set; }
        public decimal FinalPrice { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string Notice { get; set; } = string.Empty;

        // Optional: include Customer and Driver information if needed
        public CustomerDTO Customer { get; set; }
        public DriverDTO Driver { get; set; }
    }

}
