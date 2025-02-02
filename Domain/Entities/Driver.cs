using DeliverySystem.ClientUI.Enums;

namespace Domain.Entities
{
    public class Driver : Person
    {
        public decimal CommissionRate { get; set; }
        public Shift Shift { get; set; } 
        public ICollection<Order>? Orders { get; set; }

        public string BloodType { get; set; }= string.Empty;
        public string EmergencyPhone { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
    }

}
