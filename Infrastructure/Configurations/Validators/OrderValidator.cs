using Domain.Entities;
using FluentValidation;

namespace Infrastructure.Configurations.Validators
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(order => order.CustomerName)
                .NotEmpty().WithMessage("Customer name is required.")
                .MaximumLength(50).WithMessage("Customer name cannot exceed 50 characters.");

            RuleFor(order => order.CustomerPhone)
                .NotEmpty().WithMessage("Customer phone number is required.")
                .MaximumLength(15).WithMessage("Customer phone number cannot exceed 15 characters.");

            RuleFor(order => order.DeliveryAddress)
                .NotEmpty().WithMessage("Delivery address is required.")
                .MaximumLength(200).WithMessage("Delivery address cannot exceed 200 characters.");

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
