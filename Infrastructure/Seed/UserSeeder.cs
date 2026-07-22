using Application.Helpers;
using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Seed
{
    /// <summary>
    /// Ensures the two system users exist: Admin and Staff.
    /// </summary>
    public static class UserSeeder
    {
        public const string AdminEmail = "admin@delivery.local";
        public const string StaffEmail = "staff@delivery.local";
        public const string DefaultAdminPassword = "Admin@123";
        public const string DefaultStaffPassword = "Staff@123";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("UserSeeder");

            await EnsureUserAsync(db, logger,
                email: AdminEmail,
                fullName: "Admin",
                role: Roles.Admin,
                password: DefaultAdminPassword,
                phone: "700000001");

            await EnsureUserAsync(db, logger,
                email: StaffEmail,
                fullName: "Staff",
                role: Roles.Staff,
                password: DefaultStaffPassword,
                phone: "700000002");
        }

        private static async Task EnsureUserAsync(
            ApplicationDbContext db,
            ILogger? logger,
            string email,
            string fullName,
            string role,
            string password,
            string phone)
        {
            var exists = await db.Users.AnyAsync(u => u.Email == email);
            if (exists)
            {
                return;
            }

            IdentityHelpers.GetPasswordHashAndSalt(password, out var hash, out var salt);
            db.Users.Add(new User
            {
                FullName = fullName,
                Email = email,
                Role = role,
                PasswordHash = hash,
                PasswordSalt = salt,
                PhoneNumber1 = phone,
                PhoneNumber2 = string.Empty,
                Address = string.Empty,
                IsActive = true
            });
            await db.SaveChangesAsync();
            logger?.LogInformation("Seeded user {Email} with role {Role}", email, role);
        }
    }
}
