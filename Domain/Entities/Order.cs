namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public DateTime DeliveryTime { get; set; }  // Scheduled delivery time
        public string PaymentMethod { get; set; } = string.Empty;   // Payment options (e.g., Cash, Deposit)
        public decimal DeliveryFee { get; set; }      // Delivery fee for the order
        public decimal CouponDiscount { get; set; }   // Coupon discount applied to the order
        public decimal FinalPrice { get; set; }       // Price after applying discount
        public int DriverId { get; set; }             // ID of the assigned driver
        public Driver Driver { get; set; }    // Navigation property for the assigned driver
        public string OrderStatus { get; set; } = string.Empty;      // Current status (e.g., Submitted, In Progress, Delivered)
        public string Notice { get; set; } = string.Empty;        // Additional notes for the order
    }

}
