using System.ComponentModel.DataAnnotations;

namespace Application.DTO
{
    public class PersonDTO : BaseEntityDTO
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        public string FullName { get; set; } = string.Empty;
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^\d{0,9}$", ErrorMessage = "رقم الهاتف يجب ألا يتجاوز 9 أرقام")]
        public string PhoneNumber1 { get; set; }
        public string PhoneNumber2 { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true; 

    }

}
