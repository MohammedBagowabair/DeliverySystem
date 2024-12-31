using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Models
{
    public class PagedList<T>
    {
        public List<T> Entities { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages => (int)Math.Ceiling((float)TotalCount / (float)PageSize);
        public bool HasNextPage => Page * PageSize < TotalCount;
        public bool HasPreviousPage => Page > 1;

        public int CurrentPage { get; set; }

        public PagedList(int totalCount, List<T> entities, int page, int pageSize)
        {
            TotalCount = totalCount;
            Entities = entities;
            Page = page;
            PageSize = pageSize;
        }
    }
}
