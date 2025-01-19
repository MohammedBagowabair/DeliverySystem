using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Customer
{
    public record GetAllSearchPagedCustomersQuery : BasePagedQueryRequest<Domain.Entities.Customer>
    {
        public readonly string _searchTerm;
        public GetAllSearchPagedCustomersQuery(string searchTerm, int Page, int PageSize) : base( Page, PageSize)
        {
            _searchTerm = searchTerm;
        }
    }
}
