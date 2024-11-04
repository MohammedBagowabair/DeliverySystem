using Application.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        public IApplicationDbContext _dbContext;
        public OrderService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Order> Create(Order order)
        {

            return await _dbContext.AddAsync<Order>(order);
        }

        public async Task<bool> Delete(int id)
        {
            return await _dbContext.DeleteAsync<Order>(id);
        }

        public async Task<IEnumerable<Order>> GetAll()
        {
            return await _dbContext.GetAsync<Order>();
        }

        public async Task<Order> GetById(int id)
        {
            return (await _dbContext.GetAsync<Order>(x => x.Id == id))?.FirstOrDefault();
        }

        public async Task Update(Order order)
        {
            await _dbContext.UpdateAsync<Order>(order);
        }
    }
}
