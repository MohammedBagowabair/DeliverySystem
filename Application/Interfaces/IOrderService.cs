using Domain.Common.Models;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        Task<Order> GetById(int id);
        Task<IEnumerable<Order>> GetAll();
        Task<bool> Delete(int id);
        Task Update(Order order);
        Task<Order> Create(Order order);
        Task<PagedList<Order>> GetAllPagedAsync(int page, int PageSize);

        Task<PagedList<Order>> SearchOrdersAsync(string searchTerm, int page, int pageSize);
    }
}
