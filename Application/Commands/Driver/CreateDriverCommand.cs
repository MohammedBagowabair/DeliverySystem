using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Driver
{
    public record  CreateDriverCommand (Domain.Entities.Driver Driver):IRequest<Domain.Entities.Driver>;
}
