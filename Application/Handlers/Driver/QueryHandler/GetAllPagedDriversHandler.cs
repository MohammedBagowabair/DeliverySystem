using Application.Interfaces;
using Application.Queries.Driver;
using Domain.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Driver.QueryHandler
{
    public class GetAllPagedDriversHandler(IDriverService _driverService) : IRequestHandler<GetAllPagedDriversQuery, PagedList<Domain.Entities.Driver>>
    {
        public async Task<PagedList<Domain.Entities.Driver>> Handle(GetAllPagedDriversQuery request, CancellationToken cancellationToken)
        {
            return await _driverService.GetAllPagedAsync(request.Page, request.PageSize);
        }
    }
}
