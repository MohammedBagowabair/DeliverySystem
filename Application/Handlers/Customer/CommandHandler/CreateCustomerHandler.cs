using Application.Commands.Customer;
using Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Customer.CommandHandler
{
    public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Domain.Entities.Customer>
    {
        private readonly ICustomerService _customerService;

        public CreateCustomerHandler(ICustomerService customerService)
        {
            _customerService = customerService;
        }
        public async Task<Domain.Entities.Customer> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            return await _customerService.Create(request.Customer);
        }
    }
}
