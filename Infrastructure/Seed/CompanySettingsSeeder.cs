using Application.Helpers;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Seed
{
    public static class CompanySettingsSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("CompanySettingsSeeder");

            var exists = await db.Set<CompanySettings>().AnyAsync();
            if (exists)
            {
                return;
            }

            db.Set<CompanySettings>().Add(RevenueSplit.DefaultSettings());
            await db.SaveChangesAsync();
            logger?.LogInformation("Seeded default company settings.");
        }
    }
}
