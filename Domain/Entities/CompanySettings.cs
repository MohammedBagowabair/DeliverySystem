namespace Domain.Entities
{
    /// <summary>Singleton row (Id=1) for company revenue division rules.</summary>
    public class CompanySettings : BaseEntity
    {
        /// <summary>Share of total revenue reserved for expenses/fund (0–1).</summary>
        public decimal ExpensesShare { get; set; }

        /// <summary>Share of company profit pool for managers (0–1).</summary>
        public decimal ManagersShare { get; set; }

        /// <summary>Share of company profit pool for management (0–1).</summary>
        public decimal ManagementShare { get; set; }

        /// <summary>Share of company profit pool kept as company reserve (0–1).</summary>
        public decimal CompanyReserveShare { get; set; }
    }
}
