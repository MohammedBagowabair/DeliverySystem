using Domain.Entities;
using FluentValidation;

namespace Infrastructure.Configurations.Validators
{
    public class CustomerValidator : PersonValidator<Customer>
    {
        public CustomerValidator()
        {
            RuleFor(customer => customer.Orders)
                .NotNull().WithMessage("Orders cannot be null.");
        }
    }
}