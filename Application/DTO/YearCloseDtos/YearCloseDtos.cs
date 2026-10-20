using Application.DTO.CompanyFundDtos;

namespace Application.DTO.YearCloseDtos
{
    public class FundShareSnapshotDTO
    {
        public int FundId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Share { get; set; }
    }

    public class ClosedBusinessYearDTO
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public DateTime ClosedAt { get; set; }
        public int ClosedByUserId { get; set; }
        public string? ClosedByUserName { get; set; }
        public string? Notes { get; set; }
        public int TotalOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CanceledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal DriverProfit { get; set; }
        public List<FundShareSnapshotDTO> FundShares { get; set; } = new();
    }

    public class CloseBusinessYearRequest
    {
        public int Year { get; set; }
        public string? Notes { get; set; }
    }

    public class YearCloseEligibilityDTO
    {
        public int Year { get; set; }
        public bool IsClosed { get; set; }
        public bool CanClose { get; set; }
        public int ProcessingCount { get; set; }
        public int DeliveredCount { get; set; }
        public int CanceledCount { get; set; }
        public int TotalOrders { get; set; }
        public string? BlockReason { get; set; }
    }

    public class YearArchiveDriverSummaryDTO
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public int DeliveredCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class YearArchiveDetailDTO
    {
        public ClosedBusinessYearDTO Year { get; set; } = new();
        public List<YearArchiveDriverSummaryDTO> Drivers { get; set; } = new();
        public bool IsClosed { get; set; }
    }

    public class YearProfitDTO
    {
        public int Year { get; set; }
        public bool UsedSnapshotFunds { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal DriverProfit { get; set; }
        public decimal RemainingAfterDrivers { get; set; }
        public decimal CompanyAllocatedTotal { get; set; }
        public List<FundAmountDTO> Funds { get; set; } = new();
    }

    public class ActiveYearDTO
    {
        public int ActiveYear { get; set; }
        public DateTime YearStart { get; set; }
        public DateTime YearEnd { get; set; }
        public DateTime YemenToday { get; set; }
    }
}
