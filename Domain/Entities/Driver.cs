using DeliverySystem.ClientUI.Enums;

namespace Domain.Entities
{
    public class Driver : Person
    {
        public decimal CommissionRate { get; set; }
        public Shift Shift { get; set; } 
        public ICollection<Order>? Orders { get; set; }
    }

}
