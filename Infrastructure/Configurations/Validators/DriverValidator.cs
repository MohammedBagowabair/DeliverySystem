using Domain.Entities;
using FluentValidation;

namespace Infrastructure.Configurations.Validators
{
    public class DriverValidator : PersonValidator<Driver>
    {
        public DriverValidator()
        {
            RuleFor(driver => driver.CommissionRate)
                .InclusiveBetween(0, 1).WithMessage("Commission rate must be between 0 and 1.");

            RuleFor(driver => driver.Shift)
                .NotEmpty().WithMessage("Shift is required.")
                .Must(shift => shift == "Morning" || shift == "Night").WithMessage("Shift must be 'Morning' or 'Night'.");
        }
    }
}