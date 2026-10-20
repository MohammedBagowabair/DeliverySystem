namespace Domain.Enums
{
    /// <summary>
    /// Discount applied only to the company's share of order revenue (never to the driver).
    /// </summary>
    public enum DiscountType
    {
        None = 0,
        Percentage = 1,
        Fixed = 2
    }
}
