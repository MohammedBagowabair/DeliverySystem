namespace Application.Helpers
{
    /// <summary>
    /// Canonical order period windows (inclusive bounds for DeliveryTime filters).
    /// Today: calendar day. Week: last 7 days through end of today. Month: current month MTD.
    /// </summary>
    public static class OrderDateRanges
    {
        public static DateTime EndOfToday => DateTime.Today.AddDays(1).AddTicks(-1);

        public static (DateTime Start, DateTime End) Today()
            => (DateTime.Today, EndOfToday);

        public static (DateTime Start, DateTime End) Last7Days()
            => (DateTime.Today.AddDays(-7), EndOfToday);

        public static (DateTime Start, DateTime End) CurrentMonthMtd()
            => (new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), EndOfToday);
    }
}
