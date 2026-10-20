namespace Application.Helpers
{
    /// <summary>
    /// Canonical order period windows in Yemen local time (inclusive DeliveryTime bounds).
    /// "Today" / week / month are anchored on the controlled business day when provided
    /// (not the calendar clock), so lagging workdays still show correct stats.
    /// Pass <paramref name="activeYear"/> to keep ranges inside the open business year.
    /// </summary>
    public static class OrderDateRanges
    {
        public static DateTime EndOfToday => YemenTime.EndOfToday;

        public static (DateTime Start, DateTime End) Year(int year)
            => (YemenTime.StartOfYear(year), YemenTime.EndOfYear(year));

        public static (DateTime Start, DateTime End) ForDay(DateTime day)
            => (YemenTime.StartOfDay(day), YemenTime.EndOfDay(day));

        public static (DateTime Start, DateTime End) Today(DateTime businessDay, int? activeYear = null)
        {
            var range = ForDay(businessDay);
            return activeYear.HasValue ? ClampToYear(range, activeYear.Value) : range;
        }

        /// <summary>Calendar today — prefer <see cref="Today(DateTime, int?)"/> with business day.</summary>
        public static (DateTime Start, DateTime End) Today(int? activeYear = null)
            => Today(YemenTime.Today, activeYear);

        public static (DateTime Start, DateTime End) Last7Days(DateTime businessDay, int? activeYear = null)
        {
            var day = businessDay.Date;
            var range = (day.AddDays(-7), YemenTime.EndOfDay(day));
            return activeYear.HasValue ? ClampToYear(range, activeYear.Value) : range;
        }

        public static (DateTime Start, DateTime End) Last7Days(int? activeYear = null)
            => Last7Days(YemenTime.Today, activeYear);

        public static (DateTime Start, DateTime End) CurrentMonthMtd(DateTime businessDay, int? activeYear = null)
        {
            var day = businessDay.Date;
            var range = (new DateTime(day.Year, day.Month, 1), YemenTime.EndOfDay(day));
            return activeYear.HasValue ? ClampToYear(range, activeYear.Value) : range;
        }

        public static (DateTime Start, DateTime End) CurrentMonthMtd(int? activeYear = null)
            => CurrentMonthMtd(YemenTime.Today, activeYear);

        public static (DateTime Start, DateTime End) ClampToYear((DateTime Start, DateTime End) range, int year)
        {
            var (yearStart, yearEnd) = Year(year);
            var start = range.Start < yearStart ? yearStart : range.Start;
            var end = range.End > yearEnd ? yearEnd : range.End;
            if (start > end)
                return (yearStart, yearEnd);
            return (start, end);
        }
    }
}
