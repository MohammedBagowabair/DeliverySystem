using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Seed
{
    /// <summary>
    /// Removes all business data and non-admin users, leaving a clean database for Admin to populate.
    /// </summary>
    public static class DatabaseResetSeeder
    {
        public static async Task ResetToAdminOnlyAsync(ApplicationDbContext db, ILogger? logger = null)
        {
            logger?.LogWarning("Resetting database to admin-only baseline...");

            await db.Orders.ExecuteDeleteAsync();
            await db.Drivers.ExecuteDeleteAsync();
            await db.Customers.ExecuteDeleteAsync();

            await db.Users
                .Where(u => u.Email != UserSeeder.AdminEmail && u.Email != UserSeeder.StaffEmail)
                .ExecuteDeleteAsync();

            logger?.LogInformation(
                "Database reset complete. Remaining users: {Count}. Orders/drivers/customers cleared.",
                await db.Users.CountAsync());
        }
    }
}
