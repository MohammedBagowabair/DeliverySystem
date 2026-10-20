namespace Application.Helpers
{
    /// <summary>
    /// Yemen local time (UTC+3, no DST). Use for all "current day" business rules.
    /// </summary>
    public static class YemenTime
    {
        private static readonly TimeZoneInfo Zone = ResolveZone();

        private static TimeZoneInfo ResolveZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Aden");
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
                }
                catch (TimeZoneNotFoundException)
                {
                    return TimeZoneInfo.CreateCustomTimeZone(
                        "Yemen Standard Time",
                        TimeSpan.FromHours(3),
                        "Yemen Standard Time",
                        "Yemen Standard Time");
                }
            }
        }

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);

        public static DateTime Today => Now.Date;

        public static DateTime StartOfToday => Today;

        public static DateTime EndOfToday => Today.AddDays(1).AddTicks(-1);

        public static DateTime StartOfDay(DateTime date) => date.Date;

        public static DateTime EndOfDay(DateTime date) => date.Date.AddDays(1).AddTicks(-1);

        public static DateTime StartOfYear(int year) => new(year, 1, 1);

        public static DateTime EndOfYear(int year) => EndOfDay(new DateTime(year, 12, 31));

        /// <summary>Noon Yemen time on the given calendar date (for backfilled orders).</summary>
        public static DateTime NoonOn(DateTime date) => date.Date.AddHours(12);

        public static bool IsBeforeToday(DateTime deliveryTime) => deliveryTime < StartOfToday;
    }
}
