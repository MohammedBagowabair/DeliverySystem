using Application.DTO.OrderDtos;
using Application.Interfaces;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

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

        public async Task<PagedList<Order>> TodayOrdersAsync(string searchTerm,int page,int pageSize,DateTime? startDate = null,DateTime? endDate = null)
        {
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();

            // Default to today's orders
            DateTime start = startDate ?? DateTime.Today; // Start of the current day
            DateTime end = endDate ?? DateTime.Today.AddDays(1); // Start of the next day

            // Build the query
            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DeliveryTime >= start &&
                    order.DeliveryTime < end 
                    );

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
                .OrderBy(order => order.Id) // Use OrderByDescending if needed
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return the paged list
            return new PagedList<Order>(totalCount, orders, page, pageSize);
        }
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
                    order.orderStatus==OrderStatus.Delivered &&
                    order.orderStatus == OrderStatus.Delivered);

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

        public async Task<PagedList<Order>> GetLastWeekOrdersAsync(string searchTerm,int page,int pageSize,DateTime? startDate = null,DateTime? endDate = null)
        {
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();

            // Set default date range for the last 7 days
            DateTime start = startDate ?? DateTime.Today.AddDays(-7); // 7 days ago
            DateTime end = endDate ?? DateTime.Today.AddDays(1);      // End of today (inclusive)

            // Build the query
            var query = _dbContext._dbContext.Set<Order>()
                .Include(order => order.Driver)
                .Include(order => order.Customer)
                .Where(order =>
                    order.DeliveryTime >= start &&
                    order.DeliveryTime < end &&
                    order.orderStatus == OrderStatus.Delivered);

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
                .OrderBy(order => order.Id) // Use OrderByDescending if needed
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return the paged list
            return new PagedList<Order>(totalCount, orders, page, pageSize);
        }
        public async Task<PagedList<Order>> GetAllOrdersAsync(string searchTerm, int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime start = startDate ?? startDate.Value;
            DateTime end = endDate ?? endDate.Value;
            if (startDate == null && endDate == null)
            {
                searchTerm = searchTerm?.Trim()?.ToLower();
                if (string.IsNullOrEmpty(searchTerm))
                {
                    //return await _dbContext._dbContext.Set<Order>().Where(x=>x.CustomerId>10).ToListAsync();
                    return await _dbContext.GetPagedAsync<Order>(page, pageSize, null, ["Driver", "Customer"], true);
                }


                // Use predicate for search
                return await _dbContext.GetPagedAsync<Order>(
                    page,
                    pageSize, d=>d.Customer.FullName.ToLower().Contains(searchTerm) || d.Driver.FullName.Contains(searchTerm) || d.Title.Contains(searchTerm), ["Driver", "Customer"], true
                );
            }
            else
            {
                // Normalize search term
                searchTerm = searchTerm?.Trim()?.ToLower();
                if (string.IsNullOrEmpty(searchTerm))
                {
                    //return await _dbContext._dbContext.Set<Order>().Where(x=>x.CustomerId>10).ToListAsync();
                    return await _dbContext.GetPagedAsync<Order>(page, pageSize, x => x.DeliveryTime >= start && x.DeliveryTime <= end, ["Driver", "Customer"], true);
                }


                // Use predicate for search
                return await _dbContext.GetPagedAsync<Order>(
                    page,
                    pageSize, d => d.DeliveryTime >= start && d.DeliveryTime <= end || d.Customer.FullName.ToLower().Contains(searchTerm) || d.Driver.FullName.Contains(searchTerm) || d.Title.Contains(searchTerm), ["Driver", "Customer"], true
                );
            }
        }

        public async Task<PagedList<Order>> GetPDFLastWeekOrdersAsync()
        {
            try
            {
                // Set default date range for the last 7 days
                DateTime start = DateTime.Today.AddDays(-7); // 7 days ago
                DateTime end = DateTime.Today.AddDays(1);    // End of today (inclusive)

                // Build the query
                var query = _dbContext._dbContext.Set<Order>()
                    .Include(order => order.Driver)
                    .Include(order => order.Customer)
                    .Where(order =>
                        order.DeliveryTime >= start &&
                        order.DeliveryTime < end &&
                        order.orderStatus == OrderStatus.Delivered);

                // Get all the orders within this period (no search term or pagination)
                var orders = await query.ToListAsync();

                // Return the list of orders in a paged format (with one page, containing all the records)
                return new PagedList<Order>(orders.Count, orders, 1, orders.Count); // Return all orders as a single page
            }
            catch (Exception ex)
            {
                // Handle errors, if any
                throw new Exception("Error fetching orders: " + ex.Message);
            }
        }




    }
}
