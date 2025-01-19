using Application.Interfaces;
using Application.Queries.Customer;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Customer.QueryHandler
{
    public class GetAllCustomersHandler(ICustomerService customerService) : IRequestHandler<GetAllCustomersQuery, IEnumerable<Domain.Entities.Customer>>
    {
        public Task<IEnumerable<Domain.Entities.Customer>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            return customerService.GetAll();
        }
    }
}
