namespace Domain.Constants
{
    /// <summary>
    /// Role permission matrix for the Delivery System.
    /// Staff: day-to-day operations. Admin: full control including users & settings.
    /// </summary>
    public static class AppPermissions
    {
        /// <summary>Orders, drivers, customers, dashboard, reports.</summary>
        public const string Operations = Roles.Staff + "," + Roles.Admin;

        /// <summary>User management, company settings, database reset.</summary>
        public const string AdminOnly = Roles.Admin;

        public const string OrdersAccess = StaffPermissions.Orders + "," + Roles.Admin;
        public const string OrdersOrReportsAccess = StaffPermissions.Orders + "," + StaffPermissions.Reports + "," + Roles.Admin;
        public const string DriversAccess = StaffPermissions.Drivers + "," + Roles.Admin;
        public const string CustomersAccess = StaffPermissions.Customers + "," + Roles.Admin;
        public const string ReportsAccess = StaffPermissions.Reports + "," + Roles.Admin;
        public const string WorkDayAccess = StaffPermissions.WorkDay + "," + Roles.Admin;
        public const string YearsAccess = StaffPermissions.Years + "," + Roles.Admin;
        public const string ViewFinanceAccess = StaffPermissions.ViewFinance + "," + Roles.Admin;
    }
}
