using Application.DTO.CompanySettingsDtos;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICompanySettingsService
    {
        Task<CompanySettings> GetAsync();
        Task<CompanySettingsDTO> UpdateAsync(UpdateCompanySettingsDTO dto);
    }
}
