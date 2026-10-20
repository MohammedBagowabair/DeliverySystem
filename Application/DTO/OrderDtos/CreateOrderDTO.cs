using Domain.Constants;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.OrderDtos
{
    public class CreateOrderDTO : BaseEntityDTO
    {

        [Required(ErrorMessage = "العميل مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار عميل")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "الموصل مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار موصل")]
        public int DriverId { get; set; }

        public DateTime DeliveryTime { get; set; }

        [Required(ErrorMessage = "اختر طريقة الدفع")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "رسوم التوصيل يجب أن تكون رقماً موجباً")]
        public decimal DeliveryFee { get; set; }

        /// <summary>Legacy; prefer DiscountType + DiscountValue.</summary>
        [Range(0, 100, ErrorMessage = "خصم القسيمة يجب أن يكون بين 0 و 100")]
        public decimal CouponDiscount { get; set; }

        public DiscountType DiscountType { get; set; } = DiscountType.None;

        [Range(0, double.MaxValue, ErrorMessage = "قيمة الخصم يجب أن تكون رقماً موجباً")]
        public decimal DiscountValue { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "السعر النهائي يجب أن تكون رقماً موجباً")]
        public decimal FinalPrice { get; set; }

        public OrderStatus orderStatus { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Notice { get; set; } = string.Empty;

    }
}
