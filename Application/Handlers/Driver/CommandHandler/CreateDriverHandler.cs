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
    public class CreateDriverHandler(IDriverService _driverService) : IRequestHandler<CreateDriverCommand, Domain.Entities.Driver>
    {
        public async Task<Domain.Entities.Driver> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
        {
            return await _driverService.Create(request.Driver);
        }
    }
}
