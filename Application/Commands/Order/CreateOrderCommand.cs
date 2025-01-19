using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Order
{
    public record CreateOrderCommand(Domain.Entities.Order Order) :IRequest<Domain.Entities.Order>;

}
