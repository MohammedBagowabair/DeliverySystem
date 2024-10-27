using Domain.Entities;
using FluentValidation;

namespace Infrastructure.Configurations.Validators
{
    public class DriverValidator : AbstractValidator<Driver>
    {
        public DriverValidator()
        {
            RuleFor(driver => driver.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(50).WithMessage("Full name cannot exceed 50 characters.");

            RuleFor(driver => driver.PhoneNumber1)
                .NotEmpty().WithMessage("Primary phone number is required.")
                .MaximumLength(15).WithMessage("Phone number cannot exceed 15 characters.");

            RuleFor(driver => driver.PhoneNumber2)
                .MaximumLength(15).WithMessage("Secondary phone number cannot exceed 15 characters.")
                .When(driver => !string.IsNullOrEmpty(driver.PhoneNumber2));

            RuleFor(driver => driver.CommissionRate)
                .InclusiveBetween(0, 1).WithMessage("Commission rate must be between 0 and 1.");

            RuleFor(driver => driver.Shift)
                .NotEmpty().WithMessage("Shift is required.")
                .Must(shift => shift == "Morning" || shift == "Night").WithMessage("Shift must be 'Morning' or 'Night'.");
        }
    }
}