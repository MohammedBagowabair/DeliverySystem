using Application.Common.Models;
using Domain.Constants;
using Domain.Exceptions;
using Infrastructure;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppPermissions.AdminOnly)]
    public class DatabaseController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _env;

        public DatabaseController(ApplicationDbContext db, IConfiguration configuration, IHostEnvironment env)
        {
            _db = db;
            _configuration = configuration;
            _env = env;
        }

        /// <summary>
        /// Deletes all orders, drivers, customers, and non-admin users. Keeps Admin + company settings.
        /// Disabled unless Database:AllowReset is true (never enable in Production).
        /// </summary>
        [HttpPost("reset")]
        public async Task<ApiResultModel<string>> ResetAsync()
        {
            if (_env.IsProduction() || !_configuration.GetValue("Database:AllowReset", false))
            {
                return new ApiResultModel<string>(
                    ErrorCodes.USER_IS_NOT_AUTHORIZED_CODE,
                    "إعادة تعيين قاعدة البيانات غير مسموحة في هذه البيئة.",
                    string.Empty);
            }

            try
            {
                await DatabaseResetSeeder.ResetToAdminOnlyAsync(_db);
                await UserSeeder.SeedAsync(HttpContext.RequestServices);
                await CompanySettingsSeeder.SeedAsync(HttpContext.RequestServices);

                return new ApiResultModel<string>(
                    "تم تنظيف قاعدة البيانات. بقي مستخدم Admin فقط — يمكنك إضافة الموصلين والعملاء والطلبات من جديد.");
            }
            catch (Exception)
            {
                return new ApiResultModel<string>(
                    ErrorCodes.SYS_UNKNOWN_ERROR_CODE,
                    ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE,
                    string.Empty);
            }
        }

        [HttpGet("stats")]
        public async Task<ApiResultModel<object>> StatsAsync()
        {
            try
            {
                var stats = new
                {
                    Users = await _db.Users.CountAsync(),
                    Drivers = await _db.Drivers.CountAsync(),
                    Customers = await _db.Customers.CountAsync(),
                    Orders = await _db.Orders.CountAsync()
                };
                return new ApiResultModel<object>(stats);
            }
            catch (Exception)
            {
                return new ApiResultModel<object>(
                    ErrorCodes.SYS_UNKNOWN_ERROR_CODE,
                    ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE,
                    null!);
            }
        }
    }
}
