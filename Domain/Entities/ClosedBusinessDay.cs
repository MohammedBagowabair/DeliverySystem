namespace Domain.Entities
{
    /// <summary>
    /// A Yemen-calendar business day that has been closed for further order entry.
    /// </summary>
    public class ClosedBusinessDay : BaseEntity
    {
        /// <summary>Calendar date (date-only) in Yemen time.</summary>
        public DateTime BusinessDate { get; set; }

        public DateTime ClosedAt { get; set; }
        public int ClosedByUserId { get; set; }
        public User? ClosedByUser { get; set; }

        public string? Notes { get; set; }

        public int TotalOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CanceledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
