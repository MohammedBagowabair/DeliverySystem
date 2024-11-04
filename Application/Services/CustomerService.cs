using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CustomerService : ICustomerService
    {
        public IApplicationDbContext _dbContext;
        public CustomerService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
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

        public async Task<bool> Delete(int id)
        {
           return await _dbContext.DeleteAsync<Customer>(id);
        }

        public async Task<IEnumerable<Customer>> GetAll()
        {
            return await _dbContext.GetAsync<Customer>();
        }

        public async Task<Customer> GetById(int id)
        {
            return (await _dbContext.GetAsync<Customer>(x => x.Id == id))?.FirstOrDefault();
        }

        public async Task Update(Customer customer)
        {
            await _dbContext.UpdateAsync(customer);
        }
    }
}
