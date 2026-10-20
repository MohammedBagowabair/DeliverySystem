using Application.DTO.CompanySettingsDtos;
using Application.Helpers;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CompanySettingsService : ICompanySettingsService
    {
        private const int SettingsId = 1;
        private readonly IApplicationDbContext _dbContext;

        public CompanySettingsService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CompanySettings> GetAsync()
        {
            var settings = await _dbContext._dbContext.Set<CompanySettings>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == SettingsId);

            if (settings is not null)
            {
                return settings;
            }

            var defaults = RevenueSplit.DefaultSettings();
            await _dbContext.AddAsync(defaults);
            return defaults;
        }

        public async Task<CompanySettingsDTO> UpdateAsync(UpdateCompanySettingsDTO dto)
        {
            ValidateShares(dto);

            var existing = await _dbContext._dbContext.Set<CompanySettings>()
                .FirstOrDefaultAsync(x => x.Id == SettingsId);

            if (existing is null)
            {
                existing = RevenueSplit.DefaultSettings();
                _dbContext._dbContext.Set<CompanySettings>().Add(existing);
            }

            existing.ExpensesShare = dto.ExpensesShare;
            existing.ManagersShare = dto.ManagersShare;
            existing.ManagementShare = dto.ManagementShare;
            existing.CompanyReserveShare = dto.CompanyReserveShare;

            await _dbContext._dbContext.SaveChangesAsync();

            return MapToDto(existing);
        }

        private static void ValidateShares(UpdateCompanySettingsDTO dto)
        {
            if (dto.ExpensesShare <= 0 || dto.ExpensesShare > 1)
            {
                throw new DeliveryCoreException(
                    "نسبة الصندوق يجب أن تكون بين 1% و 100%.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            if (dto.ManagersShare <= 0 || dto.ManagersShare > 1
                || dto.ManagementShare <= 0 || dto.ManagementShare > 1
                || dto.CompanyReserveShare <= 0 || dto.CompanyReserveShare > 1)
            {
                throw new DeliveryCoreException(
                    "نسب تقسيم أرباح الشركة يجب أن تكون بين 1% و 100%.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }

            var profitSharesTotal = dto.ManagersShare + dto.ManagementShare + dto.CompanyReserveShare;
            if (Math.Abs(profitSharesTotal - 1m) > 0.001m)
            {
                throw new DeliveryCoreException(
                    "مجموع نسب المدراء والإدارة واحتياطي الشركة يجب أن يساوي 100%.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }
        }

        private static CompanySettingsDTO MapToDto(CompanySettings settings) => new()
        {
            Id = settings.Id,
            ExpensesShare = settings.ExpensesShare,
            ManagersShare = settings.ManagersShare,
            ManagementShare = settings.ManagementShare,
            CompanyReserveShare = settings.CompanyReserveShare
        };
    }
}
