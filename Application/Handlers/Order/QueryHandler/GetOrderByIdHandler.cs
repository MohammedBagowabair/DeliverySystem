using Application.Interfaces;
using Application.Queries.Order;
using iText.StyledXmlParser.Jsoup.Nodes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Order.QueryHandler
{
    public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Domain.Entities.Order>
    {
        private readonly IOrderService _orderService;

        public GetOrderByIdHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }
        public Task<Domain.Entities.Order> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            return  _orderService.GetById(request.Id);
         }
    }
}
