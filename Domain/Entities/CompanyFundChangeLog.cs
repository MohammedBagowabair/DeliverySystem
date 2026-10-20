namespace Domain.Entities
{
    /// <summary>Audit trail for company fund create / update / delete / replace operations.</summary>
    public class CompanyFundChangeLog : BaseEntity
    {
        public int? FundId { get; set; }
        public string FundName { get; set; } = string.Empty;

        /// <summary>Created | Updated | Deleted | Replaced</summary>
        public string Action { get; set; } = string.Empty;

        public decimal? OldShare { get; set; }
        public decimal? NewShare { get; set; }

        /// <summary>Human-readable details / snapshot after the change.</summary>
        public string? Details { get; set; }

        public DateTime ChangedAt { get; set; }
        public int ChangedByUserId { get; set; }
        public User? ChangedByUser { get; set; }
    }
}
