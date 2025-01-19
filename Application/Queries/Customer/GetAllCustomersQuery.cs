using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Customer
{
    public record  GetAllCustomersQuery:IRequest<IEnumerable<Domain.Entities.Customer>>;
}
