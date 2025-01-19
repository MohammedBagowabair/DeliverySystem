using Application.Commands.Order;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Order.CommandHandler
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Domain.Entities.Order>
    {
        private readonly IOrderService _orderService;

        public CreateOrderHandler(IOrderService orderService )
        {
            _orderService = orderService;
        }
        public async Task<Domain.Entities.Order> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            return await _orderService.Create(request.Order);
        }
    }
}
