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
    public class DeleteOrderHandler(IOrderService _orderService) : IRequestHandler<DeleteOrderCommand, bool>
    {
        public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            return await _orderService.Delete(request.Id);
        }
    }
}
