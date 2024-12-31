using Domain.Entities;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces
{
    public interface IApplicationDbContext
    {
        //DbContext _dbContext { get; }
        Task<bool> DeleteAsync<T>(int id) where T : BaseEntity;
        Task UpdateAsync<T>(T entity) where T : BaseEntity;
        Task<T>AddAsync<T>(T entity) where T : BaseEntity;
        Task<IEnumerable<T>>GetAsync<T>(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> includeExpression = null
           ) where T : BaseEntity;

        Task<Domain.Common.Models.PagedList<T>> GetPagedAsync<T>(int page, int PageSize, System.Linq.Expressions.Expression<Func<T, bool>> predicate = null, string[] includes = null, bool IsOrde = false) where T : BaseEntity;
    }
}


