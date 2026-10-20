using System.Security.Claims;
using Domain.Constants;

namespace WebApi.Helpers
{
    public static class StaffAccessHelper
    {
        public static bool IsAdmin(ClaimsPrincipal user) =>
            user.IsInRole(Roles.Admin);

        public static bool IsTodayOrdersOnly(ClaimsPrincipal user) =>
            !IsAdmin(user) && user.IsInRole(StaffPermissions.TodayOrdersOnly);

        public static (DateTime Start, DateTime End) ClampToBusinessToday(
            ClaimsPrincipal user,
            DateTime? startDate,
            DateTime? endDate,
            DateTime todayStart,
            DateTime todayEnd)
        {
            if (!IsTodayOrdersOnly(user))
            {
                return (
                    startDate ?? todayStart,
                    endDate ?? todayEnd);
            }

            return (todayStart, todayEnd);
        }

        public static bool IsWithinBusinessToday(DateTime deliveryTime, DateTime todayStart, DateTime todayEnd) =>
            deliveryTime >= todayStart && deliveryTime <= todayEnd;
    }
}
