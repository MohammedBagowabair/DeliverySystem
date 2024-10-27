namespace Domain.Entities
{
    public class Driver : Person
    {
        public decimal CommissionRate { get; set; }   // Commission rate (e.g., 70%)
        public string Shift { get; set; } = string.Empty;              // Driver shift (e.g., Morning, Night)
    }

}
