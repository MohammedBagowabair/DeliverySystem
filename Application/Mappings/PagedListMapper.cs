using AutoMapper;
using Domain.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    public static class PagedListMapper
    {
        public static PagedList<TD> MapPagedList<TS, TD>(this IMapper mapper, PagedList<TS> pagedList)
        {
            var entities = pagedList.Entities;
            var entitiesDto = mapper.Map<List<TD>>(entities);
            return new PagedList<TD>(pagedList.TotalCount, entitiesDto, pagedList.Page, pagedList.PageSize);
        }
    }
}
