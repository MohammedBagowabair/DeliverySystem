using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Interfaces
{
    public interface IApplicationDbContext
    {

        Task<bool> DeleteAsync<T>(int id) where T : BaseEntity;
        Task UpdateAsync<T>(T entity) where T : BaseEntity;
        Task<T> AddAsync<T>(T entity) where T : BaseEntity;
        Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> includeExpression = null
           ) where T : BaseEntity;
    }
}


