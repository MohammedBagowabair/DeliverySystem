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
    public class DeleteCustomerHandler(ICustomerService _customerService) : IRequestHandler<DeleteCustomerCommand, bool>
    {
        public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            return await _customerService.Delete(request.Id);
        }
    }
}
