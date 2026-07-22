using Domain.Constants;

namespace Application.Helpers
{
    /// <summary>
    /// Allowed order status transitions:
    /// Processing → Delivered | Canceled
    /// Delivered → Canceled
    /// Canceled → (terminal)
    /// </summary>
    public static class OrderStatusTransitions
    {
        public static bool CanTransition(OrderStatus from, OrderStatus to)
        {
            if (from == to)
            {
                return true;
            }

            return from switch
            {
                OrderStatus.Processing => to is OrderStatus.Delivered or OrderStatus.Canceled,
                OrderStatus.Delivered => to == OrderStatus.Canceled,
                OrderStatus.Canceled => false,
                _ => false
            };
        }

        public static string DescribeAllowed(OrderStatus from) => from switch
        {
            OrderStatus.Processing => "Delivered or Canceled",
            OrderStatus.Delivered => "Canceled",
            OrderStatus.Canceled => "none (terminal)",
            _ => "none"
        };
    }
}
