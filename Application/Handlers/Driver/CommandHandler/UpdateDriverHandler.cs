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
    public class UpdateDriverHandler: IRequestHandler<UpdateDriverCommand>
    {
        private readonly IDriverService _driverService;

        public UpdateDriverHandler(IDriverService driverService)
        {
            _driverService = driverService;
        }
        public async Task Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
        {
            await _driverService.Update(request.Driver);
        }
    }
}
