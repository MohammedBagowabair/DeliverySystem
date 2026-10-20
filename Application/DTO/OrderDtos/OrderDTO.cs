using Application.DTO.CustomerDtos;
using Application.DTO.DriverDtos;
using Domain.Constants;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.OrderDtos
{
    public class OrderDTO : BaseEntityDTO
    {
        public int CustomerId { get; set; }

        [ValidateNever]
        public CustomerOrderPagedDTO? Customer { get; set; }

        /// <summary>Denormalized; prefer over Customer.FullName in UI.</summary>
        public string CustomerName { get; set; } = string.Empty;

        public int DriverId { get; set; }

        [ValidateNever]
        public DriverPagedDTO? Driver { get; set; }

        /// <summary>Denormalized; prefer over Driver.FullName in UI.</summary>
        public string DriverName { get; set; } = string.Empty;

        public int? CreatedByUserId { get; set; }

        [ValidateNever]
        public string? CreatedByUserName { get; set; }

        public DateTime DeliveryTime { get; set; }

        public PaymentMethod paymentMethod { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "رسوم التوصيل يجب أن تكون رقماً موجباً")]
        public decimal DeliveryFee { get; set; }

        [Range(0, 100, ErrorMessage = "خصم القسيمة يجب أن يكون بين 0 و 100")]
        public decimal CouponDiscount { get; set; }

        public DiscountType DiscountType { get; set; }

        public decimal DiscountValue { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DriverCommissionRate { get; set; }
        public decimal DriverAmount { get; set; }
        public decimal CompanyGrossAmount { get; set; }
        public decimal CompanyNetAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "السعر النهائي يجب أن يكون رقماً موجباً")]
        public decimal FinalPrice { get; set; }

        public OrderStatus orderStatus { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Notice { get; set; } = string.Empty;
    }
}
