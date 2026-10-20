namespace Application.DTO.BusinessDayDtos
{
    public class BusinessDayStateDTO
    {
        public DateTime CurrentBusinessDate { get; set; }
        public DateTime CalendarToday { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
        public string? UpdatedByUserName { get; set; }
        public bool IsClosed { get; set; }
        public int PendingOrderCount { get; set; }
        public bool CanAdvance { get; set; }
        public string? BlockReason { get; set; }

        /// <summary>True when business day is before calendar today.</summary>
        public bool IsLaggingBehindCalendar { get; set; }

        /// <summary>True when user should close previous day and start calendar today (no pending).</summary>
        public bool NeedsCatchUp { get; set; }

        public DateTime? SuggestedTargetDate { get; set; }
        public string? AlertTitle { get; set; }
        public string? AlertBody { get; set; }
        public string? PrimaryActionLabel { get; set; }
    }

    public class AdvanceBusinessDayRequest
    {
        public string? Notes { get; set; }

        /// <summary>
        /// Reserved for future admin override. Currently ignored — pending orders always block.
        /// </summary>
        public bool ContinueAnyway { get; set; }
    }

    public class AdvanceBusinessDayResultDTO
    {
        public bool Success { get; set; }
        public DateTime? PreviousBusinessDate { get; set; }
        public DateTime? NewBusinessDate { get; set; }
        public int PendingOrderCount { get; set; }
        public string? Message { get; set; }
        public BusinessDayStateDTO? State { get; set; }
    }
}
