using Application.Interfaces;
using Domain.Common.Models;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CustomerService : ICustomerService
    {
        public IApplicationDbContext _dbContext;
        public CustomerService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Customer>> GetAll()
        {
            return await _dbContext.GetAsync<Customer>();
        }
        public async Task<Customer> GetById(int id)
        {
            return (await _dbContext.GetAsync<Customer>(x => x.Id == id))?.FirstOrDefault();
        }

        public async Task<Customer> Create(Customer customer)
        {
            var entity = (await _dbContext.GetAsync<Customer>(x => x.PhoneNumber1 == customer.PhoneNumber1))?.FirstOrDefault();
            if (entity != null)
            {
                throw new DeliveryCoreException(ErrorCodes.USER_ALREADY_EXISTS_CODE);
            }
            return await _dbContext.AddAsync<Customer>(customer);
        }
        public async Task Update(Customer customer)
        {
            await _dbContext.UpdateAsync(customer);
        }
        public async Task<bool> Delete(int id)
        {
            var hasOrders = await _dbContext._dbContext.Set<Order>().AnyAsync(o => o.CustomerId == id);
            if (hasOrders)
            {
                throw new DeliveryCoreException(
                    "Cannot delete customer while related orders exist.",
                    ErrorCodes.NOT_ALL_RECORDS_DELETED_CODE,
                    null);
            }
           return await _dbContext.DeleteAsync<Customer>(id);
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
        public async Task<PagedList<Customer>> GetAllPagedAsync(int page, int PageSize)
        {
            return await _dbContext.GetPagedAsync<Customer>(page, PageSize);
        }
        public async Task<PagedList<Customer>> SearchCustomersAsync(string searchTerm, int page, int pageSize)
        {
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                return await _dbContext.GetPagedAsync<Customer>(page, pageSize);
            }

            // Use predicate for search
            return await _dbContext.GetPagedAsync<Customer>(
                page,
                pageSize,
                d => d.FullName.ToLower().Contains(searchTerm) || d.PhoneNumber1.Contains(searchTerm)
            );
        }


    }
}
