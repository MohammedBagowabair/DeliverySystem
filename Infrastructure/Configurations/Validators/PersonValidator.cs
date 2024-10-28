using Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations.Validators
{
    public class PersonValidator<T> : AbstractValidator<T> where T : Person
    {
        public PersonValidator()
        {
            RuleFor(person => person.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(50).WithMessage("Full name cannot exceed 50 characters.");

            RuleFor(person => person.PhoneNumber1)
                .NotEmpty().WithMessage("Primary phone number is required.")
                .MaximumLength(15).WithMessage("Phone number cannot exceed 15 characters.");

            RuleFor(person => person.PhoneNumber2)
                .MaximumLength(15).WithMessage("Secondary phone number cannot exceed 15 characters.")
                .When(person => !string.IsNullOrEmpty(person.PhoneNumber2));

            RuleFor(person => person.Address)
                .NotEmpty().WithMessage("Address is required.");
        }
    }
}
