using Application.Commands.Customer;
using Application.Commands.User;
using Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.User.CommandHandler
{
    public class DeleteUserHandler(ICustomerService _customerService) : IRequestHandler<DeleteUserCommand, bool>
    {
        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            return await _customerService.Delete(request.Id);
        }
    }
}
