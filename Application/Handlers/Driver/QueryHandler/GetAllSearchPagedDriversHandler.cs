using Application.Interfaces;
using Application.Queries.Driver;
using Application.Services;
using Domain.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Driver.QueryHandler
{
    public class GetAllSearchPagedDriversHandler(IDriverService _driverService) : IRequestHandler<GetAllSearchPagedDriversQuery, PagedList<Domain.Entities.Driver>>
    {
        public async Task<PagedList<Domain.Entities.Driver>> Handle(GetAllSearchPagedDriversQuery request, CancellationToken cancellationToken)
        {
            return await _driverService.SearchDriversAsync(request._searchTerm, request.Page, request.PageSize);

        }
    }
}
