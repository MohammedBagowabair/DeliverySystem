using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Driver
{
    public record DeleteDriverCommand(int Id):IRequest<bool>;
}
