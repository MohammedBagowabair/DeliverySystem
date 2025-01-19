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
    public class GetCustomerByIdHandler(ICustomerService customerService) : IRequestHandler<GetCustomerByIdQuery, Domain.Entities.Customer>
    {
        public Task<Domain.Entities.Customer> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            return customerService.GetById(request.Id);
        }
    }
}
