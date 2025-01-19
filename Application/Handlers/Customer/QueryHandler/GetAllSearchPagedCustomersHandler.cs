using Application.Interfaces;
using Application.Queries.Customer;
using Domain.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Customer.QueryHandler
{
    public class GetAllSearchPagedCustomersHandler(ICustomerService _customerService) : IRequestHandler<GetAllSearchPagedCustomersQuery, PagedList<Domain.Entities.Customer>>
    {
        public async Task<PagedList<Domain.Entities.Customer>> Handle(GetAllSearchPagedCustomersQuery request, CancellationToken cancellationToken)
        {
            return await  _customerService.SearchCustomersAsync(request._searchTerm, request.Page, request.PageSize);
        }
    }
}
