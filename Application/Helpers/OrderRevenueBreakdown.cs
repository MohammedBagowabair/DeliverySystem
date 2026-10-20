using Domain.Enums;

namespace Application.Helpers
{
    /// <summary>
    /// Snapshot of order money after the 70/30 split and company-only discount.
    /// </summary>
    public class OrderRevenueBreakdown
    {
        public decimal OriginalAmount { get; set; }
        public decimal DriverCommissionRate { get; set; }
        public decimal DriverAmount { get; set; }
        public decimal CompanyShareRate { get; set; }
        public decimal CompanyGrossAmount { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal CompanyNetAmount { get; set; }
        public decimal FinalCustomerAmount { get; set; }
    }
}
