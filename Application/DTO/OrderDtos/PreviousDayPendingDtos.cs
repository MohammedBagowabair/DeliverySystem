using Domain.Constants;



namespace Application.DTO.OrderDtos

{

    public class PreviousDayPendingResult

    {

        public DateTime YemenToday { get; set; }

        public int Count { get; set; }

        public List<OrderDTO> Orders { get; set; } = new();

    }



    /// <summary>Internal service payload before mapping to DTOs.</summary>

    public class PreviousDayPendingOrdersData

    {

        public DateTime YemenToday { get; set; }

        public List<Domain.Entities.Order> Orders { get; set; } = new();

    }



    public class ResolvePreviousDayOrderDTO

    {

        /// <summary>Delivered or Canceled.</summary>

        public OrderStatus NewStatus { get; set; }

    }

}


