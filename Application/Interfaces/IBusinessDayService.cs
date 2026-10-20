using Application.DTO.BusinessDayDtos;

namespace Application.Interfaces
{
    public interface IBusinessDayService
    {
        Task<DateTime> GetCurrentBusinessDateAsync();
        Task<BusinessDayStateDTO> GetStateAsync();
        Task<AdvanceBusinessDayResultDTO> AdvanceToNextDayAsync(int userId, string? notes = null, bool continueAnyway = false);
    }
}
