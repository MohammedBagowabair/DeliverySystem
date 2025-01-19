using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Driver
{
    public record GetAllPagedDriversQuery: BasePagedQueryRequest<Domain.Entities.Driver>
    {
        internal string _searchTerm;

        public GetAllPagedDriversQuery(int Page, int PageSize) : base(Page, PageSize)
        {

        }
    }
}
