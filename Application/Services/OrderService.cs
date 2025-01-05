using Application.Interfaces;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        public IApplicationDbContext _dbContext;
        public OrderService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Order>> GetAll()
        {
            return await _dbContext.GetAsync<Order>();
        }

        public async Task<Order> GetById(int id)
        {
            return (await _dbContext.GetAsync<Order>(x => x.Id == id))?.FirstOrDefault();
        }
        public async Task<Order> Create(Order order)
        {
            order.DeliveryTime = DateTime.Now;
            order.orderStatus = OrderStatus.Processing;
            return await _dbContext.AddAsync<Order>(order);
        }
        public async Task Update(Order order)
        {
            await _dbContext.UpdateAsync<Order>(order);
        }
        public async Task<bool> Delete(int id)
        {
            return await _dbContext.DeleteAsync<Order>(id);
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

        public async Task<PagedList<Order>> SearchOrdersAsync(string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                //return await _dbContext._dbContext.Set<Order>().Where(x=>x.CustomerId>10).ToListAsync();
                return await _dbContext.GetPagedAsync<Order>(page, pageSize, null, ["Driver", "Customer"], true);
            }


            // Use predicate for search
            return await _dbContext.GetPagedAsync<Order>(
                page,
                pageSize, d => d.Customer.FullName.ToLower().Contains(searchTerm) || d.Driver.FullName.Contains(searchTerm) || d.Title.Contains(searchTerm), ["Driver", "Customer"], true
            );
        }

        //public async Task<PagedList<Order>> GetAllOrdersByDriverId(int driverId,string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        //{
        //    // Normalize search term
        //    searchTerm = searchTerm?.Trim()?.ToLower();
        //    if (string.IsNullOrEmpty(searchTerm))
        //    {
        //        return await _dbContext.GetPagedAsync<Order>(page, pageSize, x=>x.DriverId== driverId, ["Driver", "Customer"]);
        //    }

        //    // Use predicate for search
        //    return await _dbContext.GetPagedAsync<Order>(
        //        page,
        //        pageSize, d => (d.Customer.FullName.ToLower().Contains(searchTerm) || d.Driver.FullName.Contains(searchTerm) || d.Title.Contains(searchTerm)
        //   )&& d.DriverId == driverId, ["Driver", "Customer"]);
        //}
        public async Task<PagedList<Order>> GetAllOrdersByDriverId(int driverId,string searchTerm,int page,int pageSize,DateTime? startDate = null,DateTime? endDate = null)
        {
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();

            // Set default date range
            DateTime start = startDate ?? DateTime.Today;
            DateTime end = endDate ?? DateTime.Now.AddDays(1);

            // Build the query
            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DriverId == driverId &&
                    order.DeliveryTime >= start &&
                    order.DeliveryTime <= end&&
                    order.orderStatus==OrderStatus.Delivered);

            // Apply search term if provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(order =>
                    order.Customer.FullName.ToLower().Contains(searchTerm) ||
                    order.Driver.FullName.ToLower().Contains(searchTerm) ||
                    order.Title.ToLower().Contains(searchTerm));
            }

            // Get total count for pagination
            int totalCount = await query.CountAsync();

            // Apply pagination
            var orders = await query
                .OrderBy(order => order.Id) // or use OrderByDescending if required
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return the paged list
            return new PagedList<Order>(totalCount, orders, page, pageSize);
        }

    }
}
