namespace Domain.Entities
{
    public class Customer : Person
    {
        public ICollection<Order>? Orders { get; set; }       // List of orders placed by the customer
    }

}
