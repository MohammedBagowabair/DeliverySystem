using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Driver
{
    public record  GetDriverByIdQuery(int Id):IRequest<Domain.Entities.Driver>;
}
