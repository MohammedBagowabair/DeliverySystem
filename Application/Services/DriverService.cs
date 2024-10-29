using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class DriverService : IDriverService
    {
        public IApplicationDbContext _dbContext;
        public DriverService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Driver> Create(Driver driver)
        {
            return await _dbContext.AddAsync<Driver>(driver);
        }

        public async Task<bool> Delete(int id)
        {
           return await _dbContext.DeleteAsync<Driver>(id);
        }

        public async Task<IEnumerable<Driver>> GetAll()
        {
            return await _dbContext.GetAsync<Driver>();
        }

        public async Task<Driver> GetById(int id)
        {
            return (await _dbContext.GetAsync<Driver>(x => x.Id == id))?.FirstOrDefault();

        }

        public async Task Update(Driver driver)
        {
            await _dbContext.UpdateAsync<Driver>(driver);
        }
    }
}
