using Application.Commands.Order;
using Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Order.CommandHandler
{
    public class UpdateOrderHandler:IRequestHandler<UpdateOrderCommand>
    {
        private readonly IOrderService _orderService;

        public UpdateOrderHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }
        public async Task Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            await _orderService.Update(request.Order);
        }
    }
}
