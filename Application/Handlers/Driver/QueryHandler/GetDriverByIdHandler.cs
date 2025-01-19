using Application.Interfaces;
using Application.Queries.Driver;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Driver.QueryHandler
{
    public class GetDriverByIdHandler(IDriverService _driverService) : IRequestHandler<GetDriverByIdQuery, Domain.Entities.Driver>
    {
        public async Task<Domain.Entities.Driver> Handle(GetDriverByIdQuery request, CancellationToken cancellationToken)
        {
            return await _driverService.GetById(request.Id);
        }
    }
}
