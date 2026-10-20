using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        /// <summary>Denormalized customer name so history never shows null after soft-deactivate.</summary>
        public string CustomerName { get; set; } = string.Empty;

        public int DriverId { get; set; }
        public Driver? Driver { get; set; }

        /// <summary>Denormalized driver name so history never shows null after soft-deactivate.</summary>
        public string DriverName { get; set; } = string.Empty;

        /// <summary>Staff/Admin user who placed the order.</summary>
        public int? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        public DateTime DeliveryTime { get; set; }

        [Required(ErrorMessage= "اختر طريقة الدفع")]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>Original order / delivery fee amount before discount (audit base).</summary>
        [Range(0, double.MaxValue, ErrorMessage = "رسوم التوصيل يجب أن تكون رقماً موجباً")]
        public decimal DeliveryFee { get; set; }

        /// <summary>Legacy percentage field; kept in sync when DiscountType is Percentage.</summary>
        [Range(0, 100, ErrorMessage = "خصم القسيمة يجب أن يكون بين 0 و 100")]
        public decimal CouponDiscount { get; set; }

        public DiscountType DiscountType { get; set; } = DiscountType.None;

        /// <summary>Percentage (0–100) or fixed money amount depending on DiscountType.</summary>
        [Range(0, double.MaxValue, ErrorMessage = "قيمة الخصم يجب أن تكون رقماً موجباً")]
        public decimal DiscountValue { get; set; }

        /// <summary>Computed discount money taken only from the company share.</summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>Driver rate snapshot used for this order (0–1).</summary>
        public decimal DriverCommissionRate { get; set; }

        /// <summary>Driver share from original amount (unaffected by discount).</summary>
        public decimal DriverAmount { get; set; }

        /// <summary>Company share before discount (typically 30% of original).</summary>
        public decimal CompanyGrossAmount { get; set; }

        /// <summary>Company share after discount.</summary>
        public decimal CompanyNetAmount { get; set; }

        /// <summary>Amount the customer pays = DriverAmount + CompanyNetAmount.</summary>
        [Range(0, double.MaxValue, ErrorMessage = "السعر النهائي يجب أن يكون رقماً موجباً")]
        public decimal FinalPrice { get; set; }

        public OrderStatus orderStatus { get; set; }


        //[StringLength(20, ErrorMessage = "Title cannot exceed 20 characters.")]
        public string Title { get; set; } = string.Empty;

        //[StringLength(200, ErrorMessage = "Notice cannot exceed 50 characters.")]
        public string Notice { get; set; } = string.Empty;

    }

}
