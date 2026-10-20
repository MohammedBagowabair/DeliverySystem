using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Seed
{
    public static class CompanyFundSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var fundService = scope.ServiceProvider.GetRequiredService<ICompanyFundService>();
            var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("CompanyFundSeeder");

            await fundService.EnsureSeededAsync();
            logger?.LogInformation("Company funds ensured.");
        }
    }
}
