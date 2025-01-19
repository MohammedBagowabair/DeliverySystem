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
    public class GetAllPagedCustomersHandler(ICustomerService _customerService) : IRequestHandler<GetAllPagedCustomersQuery, PagedList<Domain.Entities.Customer>>
    {
        public async Task<PagedList<Domain.Entities.Customer>> Handle(GetAllPagedCustomersQuery request, CancellationToken cancellationToken)
        {
            return await _customerService.GetAllPagedAsync(request.Page, request.PageSize);
        }
    }
}
