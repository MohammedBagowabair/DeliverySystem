using Application.DTO.DayCloseDtos;

namespace Application.Interfaces
{
    public interface IDayCloseService
    {
        Task<bool> IsClosedAsync(DateTime businessDate);
        Task<DayCloseEligibilityDTO> GetEligibilityAsync(DateTime businessDate, bool allowOpenBusinessDay = false);
        Task<List<ClosedBusinessDayDTO>> GetClosedDaysAsync(int take = 60, int? year = null);
        Task<DayArchiveDetailDTO> GetArchiveDetailAsync(DateTime businessDate);
        Task<ClosedBusinessDayDTO> CloseDayAsync(DateTime businessDate, int closedByUserId, string? notes, bool allowOpenBusinessDay = false);
        /// <summary>Recalculate archived day totals after an Admin edits orders on a closed day.</summary>
        Task RefreshClosedDayTotalsAsync(DateTime businessDate);
        Task<byte[]> GenerateAllOrdersPdfAsync(DateTime businessDate);
        Task<byte[]> GenerateDriverPdfAsync(DateTime businessDate, int driverId);
    }
}
