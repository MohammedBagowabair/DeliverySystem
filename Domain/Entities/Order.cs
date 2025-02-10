using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int DriverId { get; set; }
        public Driver? Driver { get; set; }

        public DateTime DeliveryTime { get; set; }

        [Required(ErrorMessage= "Choose Payment method")]
        public PaymentMethod PaymentMethod { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Delivery fee must be a positive number.")]
        public decimal DeliveryFee { get; set; }

        [Range(0, 100, ErrorMessage = "Coupon discount must be between 0 and 100 percent.")]
        public decimal CouponDiscount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Final price must be a positive number.")]
        public decimal FinalPrice { get; set; }

        public OrderStatus orderStatus { get; set; }


        [StringLength(20, ErrorMessage = "Title cannot exceed 20 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Notice cannot exceed 50 characters.")]
        public string Notice { get; set; } = string.Empty;

    }

}
