namespace Application.Helpers
{
    /// <summary>
    /// Canonical revenue split for delivered orders:
    /// 70% driver / 20% company profit / 10% expenses.
    /// </summary>
    public static class RevenueSplit
    {
        public const decimal DriverShare = 0.7m;
        public const decimal CompanyProfitShare = 0.2m;
        public const decimal ExpensesShare = 0.1m;

        public static decimal DriverProfit(decimal revenue) => revenue * DriverShare;
        public static decimal CompanyProfit(decimal revenue) => revenue * CompanyProfitShare;
        public static decimal Expenses(decimal revenue) => revenue * ExpensesShare;
    }
}
