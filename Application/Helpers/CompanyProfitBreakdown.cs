using Application.DTO.CompanyFundDtos;

namespace Application.Helpers
{
    public class CompanyProfitBreakdown
    {
        public decimal TotalRevenue { get; set; }
        public decimal DriverProfit { get; set; }
        public decimal RemainingAfterDrivers { get; set; }
        public List<FundAmountDTO> Funds { get; set; } = new();

        /// <summary>Sum of all fund amounts (= RemainingAfterDrivers when shares total 100%).</summary>
        public decimal CompanyAllocatedTotal => Funds.Sum(f => f.Amount);

        // Legacy aliases for gradual UI migration (matched by common Arabic names).
        public decimal Expenses => AmountByNameContains("صندوق", "مصروف", "صيانة", "maintenance", "fund", "expense");
        public decimal CompanyProfitPool => CompanyAllocatedTotal - Expenses;
        public decimal ManagersAmount => AmountByNameContains("مدراء", "managers");
        public decimal ManagementAmount => AmountByNameContains("إدارة", "ادارة", "management");
        public decimal CompanyReserveAmount => AmountByNameContains("احتياطي", "reserve");

        private decimal AmountByNameContains(params string[] needles)
        {
            foreach (var fund in Funds)
            {
                var name = fund.Name ?? string.Empty;
                if (needles.Any(n => name.Contains(n, StringComparison.OrdinalIgnoreCase)))
                    return fund.Amount;
            }
            return 0;
        }
    }
}
