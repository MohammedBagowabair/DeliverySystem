using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.OrderDtos
{
    public class CreateOrderDTO : BaseEntityDTO
    {

        [Required(ErrorMessage = "Customer Id is required.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Driver Id is required.")]
        public int DriverId { get; set; }

        public DateTime DeliveryTime { get; set; }

        [Required(ErrorMessage = "Choose Payment method")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Delivery fee must be a positive number.")]
        public decimal DeliveryFee { get; set; }

        [Range(0, 100, ErrorMessage = "Coupon discount must be between 0 and 100 percent.")]
        public decimal CouponDiscount { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Final price must be a positive number.")]
        public decimal FinalPrice { get; set; }

        public OrderStatus orderStatus { get; set; }

        //[StringLength(20, ErrorMessage = "Title cannot exceed 20 characters.")]
        public string Title { get; set; } = string.Empty;

        //[StringLength(50, ErrorMessage = "Notice cannot exceed 50 characters.")]
        public string Notice { get; set; } = string.Empty;

    }
}
