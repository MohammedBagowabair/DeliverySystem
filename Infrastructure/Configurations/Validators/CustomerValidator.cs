using Domain.Entities;
using FluentValidation;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(customer => customer.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(50).WithMessage("Full name cannot exceed 50 characters.");

        RuleFor(customer => customer.PhoneNumber1)
            .NotEmpty().WithMessage("Primary phone number is required.")
            .MaximumLength(15).WithMessage("Phone number cannot exceed 15 characters.");

        RuleFor(customer => customer.PhoneNumber2)
            .MaximumLength(15).WithMessage("Secondary phone number cannot exceed 15 characters.")
            .When(customer => !string.IsNullOrEmpty(customer.PhoneNumber2));

        RuleFor(customer => customer.Address)
            .NotEmpty().WithMessage("Address is required.");
    }
}
