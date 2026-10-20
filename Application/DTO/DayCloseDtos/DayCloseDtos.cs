namespace Application.DTO.DayCloseDtos
{
    public class ClosedBusinessDayDTO
    {
        public int Id { get; set; }
        public DateTime BusinessDate { get; set; }
        public DateTime ClosedAt { get; set; }
        public int ClosedByUserId { get; set; }
        public string? ClosedByUserName { get; set; }
        public string? Notes { get; set; }
        public int TotalOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CanceledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CloseBusinessDayRequest
    {
        public DateTime BusinessDate { get; set; }
        public string? Notes { get; set; }
    }

    public class DayCloseEligibilityDTO
    {
        public DateTime BusinessDate { get; set; }
        public bool IsClosed { get; set; }
        public bool CanClose { get; set; }
        public int ProcessingCount { get; set; }
        public int DeliveredCount { get; set; }
        public int CanceledCount { get; set; }
        public int TotalOrders { get; set; }
        public string? BlockReason { get; set; }
    }

    public class DayArchiveDriverSummaryDTO
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public int DeliveredCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DayArchiveDetailDTO
    {
        public ClosedBusinessDayDTO Day { get; set; } = new();
        public List<DayArchiveDriverSummaryDTO> Drivers { get; set; } = new();
    }
}
