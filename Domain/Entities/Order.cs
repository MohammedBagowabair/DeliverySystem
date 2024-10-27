using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        public string CustomerName { get; set; }      // Customer's name
        public string CustomerPhone { get; set; }     // Customer's phone number
        public string DeliveryAddress { get; set; }   // Delivery address for the order
        public DateTime DeliveryTime { get; set; }    // Scheduled delivery time
        public string PaymentMethod { get; set; }     // Payment options (e.g., Cash, Deposit)
        public decimal DeliveryFee { get; set; }      // Delivery fee for the order
        public decimal CouponDiscount { get; set; }   // Coupon discount applied to the order
        public decimal FinalPrice { get; set; }       // Price after applying discount

        public int DriverId { get; set; }             // ID of the assigned driver
        public Driver AssignedDriver { get; set; }    // Navigation property for the assigned driver
        public string OrderStatus { get; set; }       // Current status (e.g., Submitted, In Progress, Delivered)
        public string Notice { get; set; }            // Additional notes for the order
    }

}
