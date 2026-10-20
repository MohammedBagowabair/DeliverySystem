using Domain.Constants;

namespace Application.Helpers
{
    /// <summary>
    /// Allowed order status transitions (while the business day is open):
    /// Processing → Delivered | Canceled
    /// Delivered → Processing | Canceled
    /// Canceled → Processing | Delivered
    /// After day close, reopen transitions are blocked in OrderService.
    /// </summary>
    public static class OrderStatusTransitions
    {
        public static bool CanTransition(OrderStatus from, OrderStatus to)
        {
            if (from == to)
                return true;

            return from switch
            {
                OrderStatus.Processing => to is OrderStatus.Delivered or OrderStatus.Canceled,
                OrderStatus.Delivered => to is OrderStatus.Processing or OrderStatus.Canceled,
                OrderStatus.Canceled => to is OrderStatus.Processing or OrderStatus.Delivered,
                _ => false
            };
        }

        /// <summary>
        /// Reopening / correcting a finalized status — only allowed before day close.
        /// </summary>
        public static bool IsReopenTransition(OrderStatus from, OrderStatus to)
        {
            if (from == to)
                return false;

            return from switch
            {
                OrderStatus.Canceled => to is OrderStatus.Processing or OrderStatus.Delivered,
                OrderStatus.Delivered => to == OrderStatus.Processing,
                _ => false
            };
        }

        public static string DescribeAllowed(OrderStatus from) => from switch
        {
            OrderStatus.Processing => "تم التوصيل أو ملغى",
            OrderStatus.Delivered => "قيد المعالجة أو ملغى",
            OrderStatus.Canceled => "قيد المعالجة أو تم التوصيل (قبل إغلاق اليوم)",
            _ => "لا يوجد"
        };
    }
}
