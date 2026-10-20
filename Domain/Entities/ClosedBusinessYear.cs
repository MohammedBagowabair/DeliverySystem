namespace Domain.Entities
{
    /// <summary>
    /// A Yemen calendar year that has been closed; live ops switch to the next open year.
    /// Orders are not moved or deleted — only filtered by year.
    /// </summary>
    public class ClosedBusinessYear : BaseEntity
    {
        /// <summary>Calendar year (e.g. 2026).</summary>
        public int Year { get; set; }

        public DateTime ClosedAt { get; set; }
        public int ClosedByUserId { get; set; }
        public User? ClosedByUser { get; set; }

        public string? Notes { get; set; }

        public int TotalOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CanceledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal DriverProfit { get; set; }

        /// <summary>
        /// JSON array of fund shares at close time:
        /// [{ "FundId", "Name", "Share" }, ...]
        /// </summary>
        public string FundSharesJson { get; set; } = "[]";
    }
}
