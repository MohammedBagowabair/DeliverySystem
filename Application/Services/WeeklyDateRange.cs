using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class WeeklyDateRange
    {
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        public WeeklyDateRange()
        {
            SetCurrentWeekRange();
        }

        private void SetCurrentWeekRange()
        {
            DateTime today = DateTime.Today;
            int year = today.Year;
            int month = today.Month;

            // Get total days in current month
            int daysInMonth = DateTime.DaysInMonth(year, month);

            // Define week start days (1-7, 8-14, 15-21, etc.)
            int weekStart = ((today.Day - 1) / 7) * 7 + 1;
            int weekEnd = Math.Min(weekStart + 6, daysInMonth); // Ensure it doesn't exceed month end

            // Set the range
            StartDate = new DateTime(year, month, weekStart);
            EndDate = new DateTime(year, month, weekEnd);
        }

        public override string ToString()
        {
            return $"Current Week: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
        }
    }

}
