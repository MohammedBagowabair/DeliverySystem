using Application.Commands.Driver;
using Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Driver.CommandHandler
{
    public class DeleteDriverHandler(IDriverService _driverService) : IRequestHandler<DeleteDriverCommand, bool>
    {
        public async Task<bool> Handle(DeleteDriverCommand request, CancellationToken cancellationToken)
        {
            return await _driverService.Delete(request.Id);
        }
    }
}
