using Domain.Entities;
using FluentValidation;

namespace Infrastructure.Configurations.Validators
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(order => order.Customer)
                .NotNull().WithMessage("Customer is required.")
                .SetValidator(new CustomerValidator()); // Uses CustomerValidator

            RuleFor(order => order.Driver)
                .NotNull().WithMessage("Driver is required.")
                .SetValidator(new DriverValidator()); // Uses DriverValidator

            RuleFor(order => order.DeliveryTime)
                .GreaterThanOrEqualTo(DateTime.Now).WithMessage("Delivery time must be in the future.");

            RuleFor(order => order.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required.");

            RuleFor(order => order.DeliveryFee)
                .InclusiveBetween(0, 100000).WithMessage("Delivery fee must be between 0 and 100000.");

            RuleFor(order => order.CouponDiscount)
                .InclusiveBetween(0, 100000).WithMessage("Coupon discount must be between 0 and 100000.");

            RuleFor(order => order.FinalPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Final price must be a positive value.");

            RuleFor(order => order.OrderStatus)
                .NotEmpty().WithMessage("Order status is required.");

            RuleFor(order => order.Notice)
                .MaximumLength(500).WithMessage("Notice cannot exceed 500 characters.");
        }
    }
}
