namespace Domain.Entities
{
    /// <summary>
    /// Named share of revenue remaining after driver commission.
    /// All active funds' Share values must sum to 1 (100%).
    /// </summary>
    public class CompanyFund : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Fraction of post-driver remainder (0–1).</summary>
        public decimal Share { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
        public User? UpdatedByUser { get; set; }
    }
}
