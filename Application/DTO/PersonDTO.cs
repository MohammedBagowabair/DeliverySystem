using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class PersonDTO : BaseEntityDTO
    {
        [Required(ErrorMessage = "FullName is required.")]
        public string FullName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\d{0,9}$", ErrorMessage = "Phone number must be up to 9 digits.")]
        public string PhoneNumber1 { get; set; }
        public string PhoneNumber2 { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true; 

    }

}
