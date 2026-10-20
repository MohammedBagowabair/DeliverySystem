using Application.DTO.YearCloseDtos;
using Domain.Common.Models;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IYearCloseService
    {
        Task<int> GetActiveYearAsync();
        Task<ActiveYearDTO> GetActiveYearInfoAsync();
        Task<bool> IsClosedAsync(int year);
        Task<YearCloseEligibilityDTO> GetEligibilityAsync(int year);
        Task<List<ClosedBusinessYearDTO>> GetClosedYearsAsync();
        Task<YearArchiveDetailDTO> GetArchiveDetailAsync(int year);
        Task<ClosedBusinessYearDTO> CloseYearAsync(int year, int closedByUserId, string? notes);
        Task<YearProfitDTO> GetYearProfitAsync(int year);
        Task<PagedList<Order>> GetYearOrdersAsync(int year, int page, int pageSize);
        Task<byte[]> GenerateAllOrdersPdfAsync(int year);
        Task<byte[]> GenerateDriverPdfAsync(int year, int driverId);
    }
}
