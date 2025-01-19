using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Customer
{
    public record CreateCustomerCommand(Domain.Entities.Customer Customer):IRequest<Domain.Entities.Customer>;

}
