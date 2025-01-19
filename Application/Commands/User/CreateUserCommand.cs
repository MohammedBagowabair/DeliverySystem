using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.User
{
    public record CreateUserCommand(Domain.Entities.User User) : IRequest<Domain.Entities.User>;

}
