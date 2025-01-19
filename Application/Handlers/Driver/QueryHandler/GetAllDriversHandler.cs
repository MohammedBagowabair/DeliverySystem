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
    public class GetAllDriversHandler(IDriverService _driverService) : IRequestHandler<GetAllDriversQuery, IEnumerable<Domain.Entities.Driver>>
    {
        public Task<IEnumerable<Domain.Entities.Driver>> Handle(GetAllDriversQuery request, CancellationToken cancellationToken)
        {
            return _driverService.GetAll();
        }
    }
}
