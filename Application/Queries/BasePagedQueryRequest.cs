using Domain.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    public abstract record BasePagedQueryRequest<T>(int Page, int PageSize) : IRequest<PagedList<T>>;
}
