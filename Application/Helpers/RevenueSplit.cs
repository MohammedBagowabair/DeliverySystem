using Application.DTO.CompanyFundDtos;
using Domain.Entities;

namespace Application.Helpers
{
    /// <summary>
    /// Revenue split for delivered orders:
    /// 1) Driver commission from original order amount (default 70%).
    /// 2) Company share (default 30%) then fund allocation; discounts reduce company share only
    ///    (see <see cref="OrderRevenueCalculator"/>).
    /// </summary>
    public static class RevenueSplit
    {
        /// <summary>Central default: driver receives 70% of original order revenue.</summary>
        public const decimal DefaultDriverCommissionRate = 0.70m;

        /// <summary>Central default: company receives the remainder (30%).</summary>
        public const decimal DefaultCompanyShareRate = 1m - DefaultDriverCommissionRate;

        public const decimal ShareTolerance = 0.001m;

        public const decimal DefaultExpensesShare = 0.10m;
        public const decimal DefaultManagersShare = 0.50m;
        public const decimal DefaultManagementShare = 0.20m;
        public const decimal DefaultCompanyReserveShare = 0.30m;

        public static CompanySettings DefaultSettings() => new()
        {
            ExpensesShare = DefaultExpensesShare,
            ManagersShare = DefaultManagersShare,
            ManagementShare = DefaultManagementShare,
            CompanyReserveShare = DefaultCompanyReserveShare
        };

        /// <summary>Flatten nested CompanySettings into funds that sum to 100% of post-driver remainder.</summary>
        public static List<(string Name, decimal Share, string? Description)> DefaultFundsFromSettings(CompanySettings settings)
        {
            var expenses = settings.ExpensesShare;
            var pool = Math.Max(0, 1m - expenses);
            return new List<(string, decimal, string?)>
            {
                ("الصندوق", expenses, "صندوق الشركة من المتبقي بعد عمولة الموصل"),
                ("المدراء", pool * settings.ManagersShare, "حصة المدراء"),
                ("الإدارة", pool * settings.ManagementShare, "حصة الإدارة"),
                ("احتياطي الشركة", pool * settings.CompanyReserveShare, "احتياطي الشركة")
            };
        }

        public static decimal DriverProfitForOrder(decimal finalPrice, decimal commissionRate)
        {
            var rate = commissionRate > 0 ? commissionRate : DefaultDriverCommissionRate;
            return finalPrice * rate;
        }

        public static CompanyProfitBreakdown Calculate(
            decimal totalRevenue,
            decimal totalDriverProfit,
            IReadOnlyList<CompanyFund> funds)
        {
            var remaining = Math.Max(0, totalRevenue - totalDriverProfit);
            var active = funds
                .Where(f => f.IsActive)
                .OrderBy(f => f.SortOrder)
                .ThenBy(f => f.Id)
                .ToList();

            var result = new CompanyProfitBreakdown
            {
                TotalRevenue = totalRevenue,
                DriverProfit = totalDriverProfit,
                RemainingAfterDrivers = remaining
            };

            if (active.Count == 0)
            {
                return result;
            }

            decimal allocated = 0;
            for (var i = 0; i < active.Count; i++)
            {
                var fund = active[i];
                decimal amount;
                if (i == active.Count - 1)
                {
                    // Last fund absorbs rounding remainder so totals stay exact.
                    amount = Math.Max(0, remaining - allocated);
                }
                else
                {
                    amount = Math.Round(remaining * fund.Share, 2, MidpointRounding.AwayFromZero);
                    allocated += amount;
                }

                result.Funds.Add(new FundAmountDTO
                {
                    FundId = fund.Id,
                    Name = fund.Name,
                    Share = fund.Share,
                    Amount = amount
                });
            }

            return result;
        }

        /// <summary>Legacy path: convert CompanySettings nested model into flat funds then calculate.</summary>
        public static CompanyProfitBreakdown Calculate(decimal totalRevenue, decimal totalDriverProfit, CompanySettings settings)
        {
            var mapped = DefaultFundsFromSettings(settings)
                .Select((f, i) => new CompanyFund
                {
                    Id = i + 1,
                    Name = f.Name,
                    Share = f.Share,
                    SortOrder = i,
                    IsActive = true
                })
                .ToList();
            return Calculate(totalRevenue, totalDriverProfit, mapped);
        }

        public static decimal CompanyProfitPool(decimal totalRevenue, decimal totalDriverProfit, CompanySettings settings)
            => Calculate(totalRevenue, totalDriverProfit, settings).CompanyAllocatedTotal;

        public static decimal CompanyProfitPool(decimal totalRevenue, decimal totalDriverProfit, IReadOnlyList<CompanyFund> funds)
            => Calculate(totalRevenue, totalDriverProfit, funds).CompanyAllocatedTotal;

        public static bool SharesSumToOne(IEnumerable<decimal> shares)
        {
            var total = shares.Sum();
            return Math.Abs(total - 1m) <= ShareTolerance;
        }

        [Obsolete("Use Calculate with funds.")]
        public static decimal Expenses(decimal totalRevenue) => totalRevenue * DefaultExpensesShare;

        [Obsolete("Use DriverProfitForOrder.")]
        public static decimal DriverProfit(decimal revenue) => revenue * DefaultDriverCommissionRate;

        [Obsolete("Use CompanyProfitPool with funds.")]
        public static decimal CompanyProfit(decimal revenue) => revenue * 0.2m;
    }
}
