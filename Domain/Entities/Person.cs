using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public abstract class Person : BaseEntity
    {

        public string FullName { get; set; } = string.Empty;     // Person's full name
        public string PhoneNumber1 { get; set; } = string.Empty;     // Primary phone number
        public string PhoneNumber2 { get; set; } = string.Empty;      // Secondary phone number (optional)
        public string Address { get; set; } = string.Empty;        // Address of the person
        public bool IsActive { get; set; } = true; // General active/inactive status

    }

}
