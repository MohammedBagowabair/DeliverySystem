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
    public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand>
    {
        private readonly ICustomerService _customerService;

        public UpdateCustomerHandler(ICustomerService customerService)
        {
            _customerService = customerService;
        }
        public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            await  _customerService.Update(request.Customer);
        }
    }
}
