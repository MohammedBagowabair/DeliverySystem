using Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.UserDtos
{
   public class createUserDTO:PersonDTO
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(8, ErrorMessage = "كلمة المرور يجب أن تكون 8 أحرف على الأقل")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>Allowed values: Admin, Staff only.</summary>
        [Required(ErrorMessage = "الدور مطلوب")]
        [RegularExpression("^(Admin|Staff)$", ErrorMessage = "يجب أن يكون الدور Admin أو Staff")]
        public string Role { get; set; } = Roles.Staff;

        /// <summary>Staff feature permissions (ignored for Admin — full access).</summary>
        public List<string> Permissions { get; set; } = new(StaffPermissions.DefaultStaff);
    }
}
