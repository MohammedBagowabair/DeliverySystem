using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.UserDtos
{
   public class createUserDTO:PersonDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        /// <summary>Allowed values: Admin, Staff only.</summary>
        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Admin|Staff)$", ErrorMessage = "Role must be Admin or Staff")]
        public string Role { get; set; } = Roles.Staff;
    }
}
