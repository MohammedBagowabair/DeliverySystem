using Application.Helpers;
using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Seed
{
    /// <summary>
    /// Ensures configured seed users exist. Never overwrites existing passwords.
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
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("UserSeeder");
            var env = scope.ServiceProvider.GetService<Microsoft.Extensions.Hosting.IHostEnvironment>();

            var seedUsers = config.GetSection("SeedUsers").Get<List<SeedUserOptions>>() ?? new List<SeedUserOptions>();

            if (seedUsers.Count == 0)
            {
                if (string.Equals(env?.EnvironmentName, "Production", StringComparison.OrdinalIgnoreCase))
                {
                    logger?.LogWarning("SeedUsers is empty in Production — skipping default password seed.");
                    return;
                }

                seedUsers =
                [
                    new SeedUserOptions
                    {
                        Email = AdminEmail,
                        FullName = "Admin",
                        Role = Roles.Admin,
                        Password = DefaultAdminPassword,
                        Phone = "700000001"
                    },
                    new SeedUserOptions
                    {
                        Email = StaffEmail,
                        FullName = "Staff",
                        Role = Roles.Staff,
                        Password = DefaultStaffPassword,
                        Phone = "700000002"
                    },
                    new SeedUserOptions
                    {
                        Email = "staff2@delivery.local",
                        FullName = "Staff 2",
                        Role = Roles.Staff,
                        Password = DefaultStaffPassword,
                        Phone = "700000003"
                    }
                ];
            }

            foreach (var u in seedUsers)
            {
                if (string.IsNullOrWhiteSpace(u.Email) || string.IsNullOrWhiteSpace(u.Password))
                {
                    logger?.LogWarning("Skipping seed user with missing email/password: {Email}", u.Email);
                    continue;
                }

                var role = string.IsNullOrWhiteSpace(u.Role) ? Roles.Staff : u.Role.Trim();
                if (role != Roles.Admin && role != Roles.Staff)
                    role = Roles.Staff;

                await EnsureUserAsync(
                    db,
                    logger,
                    email: u.Email.Trim(),
                    fullName: string.IsNullOrWhiteSpace(u.FullName) ? u.Email : u.FullName.Trim(),
                    role: role,
                    password: u.Password,
                    phone: u.Phone ?? string.Empty);
            }
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
                return;

            IdentityHelpers.GetPasswordHashAndSalt(password, out var hash, out var salt);
            db.Users.Add(new User
            {
                FullName = fullName,
                Email = email,
                Role = role,
                Permissions = StaffPermissions.NormalizeForRole(role, null),
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

        private sealed class SeedUserOptions
        {
            public string Email { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Role { get; set; } = Roles.Staff;
            public string Password { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
        }
    }
}
