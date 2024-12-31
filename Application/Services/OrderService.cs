using Application.Interfaces;
using Domain.Common.Models;
using Domain.Constants;
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
            order.DeliveryTime = DateTime.Now;
            order.orderStatus = OrderStatus.Processing;
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

        public async Task<int> CountAsync()
        {
            var totalRecord = await GetAll();
            int total = 0;
            foreach (var record in totalRecord)
            {
                total += 1;
            }
            return total;
        }
        public async Task<PagedList<Order>> GetAllPagedAsync(int page, int PageSize)
        {
            return await _dbContext.GetPagedAsync<Order>(page, PageSize, null, ["Driver", "Customer"]);
        }

        public async Task<PagedList<Order>> SearchOrdersAsync(string searchTerm, int page, int pageSize)
        {
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                return await _dbContext.GetPagedAsync<Order>(page, pageSize, null, ["Driver", "Customer"]);
            }

            // Use predicate for search
            return await _dbContext.GetPagedAsync<Order>(
                page,
                pageSize, d => d.Customer.FullName.ToLower().Contains(searchTerm) || d.Driver.FullName.Contains(searchTerm) || d.Title.Contains(searchTerm) , ["Driver", "Customer"]
            );
        }
    }
}
