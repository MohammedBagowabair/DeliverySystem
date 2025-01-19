using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Driver
{
    public record GetAllSearchPagedDriversQuery: BasePagedQueryRequest<Domain.Entities.Driver>
    {
        public readonly string _searchTerm;
        public GetAllSearchPagedDriversQuery(string searchTerm, int Page, int PageSize) : base(Page, PageSize)
        {
            _searchTerm = searchTerm;
        }
    }
}
