namespace Domain.Entities
{
    /// <summary>
    /// Singleton (Id=1) holding the controlled current business/working day.
    /// Does not auto-advance with the calendar clock.
    /// </summary>
    public class BusinessDayState : BaseEntity
    {
        /// <summary>Date-only current working day (Yemen calendar semantics).</summary>
        public DateTime CurrentBusinessDate { get; set; }

        public DateTime UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
        public User? UpdatedByUser { get; set; }
    }
}
