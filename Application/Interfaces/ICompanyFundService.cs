using Application.DTO.CompanyFundDtos;
using Domain.Common.Models;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICompanyFundService
    {
        Task<List<CompanyFundDTO>> GetActiveAsync();
        Task<List<CompanyFund>> GetActiveEntitiesAsync();
        Task<List<CompanyFundDTO>> ReplaceAllAsync(ReplaceCompanyFundsDTO dto, int changedByUserId);
        Task<PagedList<CompanyFundChangeLogDTO>> GetChangeHistoryAsync(int page, int pageSize);
        Task EnsureSeededAsync();
    }
}
