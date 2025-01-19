using Domain.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Customer
{
    public record  GetAllPagedCustomersQuery : BasePagedQueryRequest<Domain.Entities.Customer> 
    {
        public GetAllPagedCustomersQuery(int Page, int PageSize) :base(Page,PageSize)
        {
            
        }
    }
}
