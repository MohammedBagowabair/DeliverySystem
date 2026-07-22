using Application.Helpers;

namespace Application.Services
{
    /// <summary>
    /// Rolling last-7-days window (through end of today). Used by week order counts.
    /// </summary>
    public class WeeklyDateRange
    {
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        public WeeklyDateRange()
        {
            var range = OrderDateRanges.Last7Days();
            StartDate = range.Start;
            EndDate = range.End;
        }

        public override string ToString()
        {
            return $"Last 7 days: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
        }
    }
}
