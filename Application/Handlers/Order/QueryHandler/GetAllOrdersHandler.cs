using Application.Commands.Order;
using Application.Interfaces;
using Application.Queries.Order;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Order.QueryHandler
{
    public class GetAllOrdersHandler(IOrderService orderService) : IRequestHandler<GetAllOrdersQuery, IEnumerable<Domain.Entities.Order>>
    {
      
        public Task<IEnumerable<Domain.Entities.Order>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            return orderService.GetAll();
        }
    }
}
