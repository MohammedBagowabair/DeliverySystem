using Domain.Entities;
using FluentValidation;

namespace Infrastructure.Configurations.Validators
{

    public class UserValidator : PersonValidator<User>
    {
        public UserValidator()
        {

            RuleFor(user => user.Email)
             .NotEmpty().WithMessage("Email is required.")
             .EmailAddress().WithMessage("Invalid email format.")
             .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

            RuleFor(user => user.PasswordHash)
                .NotEmpty().WithMessage("Password hash is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

            RuleFor(user => user.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(role => role == "Admin" || role == "Customer").WithMessage("Role must be either 'Admin' or 'Customer'.");


        }
    }

}
