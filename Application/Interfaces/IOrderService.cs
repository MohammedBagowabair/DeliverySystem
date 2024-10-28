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
    }
}
