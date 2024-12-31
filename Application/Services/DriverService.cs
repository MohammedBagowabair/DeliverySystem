using Application.Interfaces;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
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

        public async Task<IEnumerable<Driver>> GetAll()
        {
            return await _dbContext.GetAsync<Driver>();
        }
        public async Task<Driver> GetById(int id)
        {
            return (await _dbContext.GetAsync<Driver>(x => x.Id == id))?.FirstOrDefault();

        }
        public async Task<Driver> Create(Driver driver)
        {
            var driverInDb = (await _dbContext.GetAsync<Driver>(x => x.PhoneNumber1==driver.PhoneNumber1))?.FirstOrDefault();
            if (driverInDb != null)
            {
                throw new DeliveryCoreException(ErrorCodes.USER_ALREADY_EXISTS_CODE);
            }
            return await _dbContext.AddAsync<Driver>(driver);
        }
        public async Task Update(Driver driver)
        {
            var driverInDb = (await _dbContext.GetAsync<Driver>(x => x.PhoneNumber1 == driver.PhoneNumber1 && driver.Id != x.Id))?.FirstOrDefault();
            if (driverInDb == null)
            {
                await _dbContext.UpdateAsync<Driver>(driver);
            }
            else
            {
                throw new DeliveryCoreException(ErrorCodes.USER_ALREADY_EXISTS_CODE);
            }

        }
        public async Task<bool> Delete(int id)
        {
           return await _dbContext.DeleteAsync<Driver>(id);
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
        public async Task<PagedList<Driver>> GetAllPagedAsync(int page, int PageSize)
        {
            return await _dbContext.GetPagedAsync<Driver>(page,PageSize);
        }
        public async Task<PagedList<Driver>> SearchDriversAsync(string searchTerm, int page, int pageSize)
        {
            // Normalize search term
            searchTerm = searchTerm?.Trim()?.ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                return await _dbContext.GetPagedAsync<Driver>(page, pageSize);
            }

            // Use predicate for search
            return await _dbContext.GetPagedAsync<Driver>(
                page,
                pageSize,
                d => d.FullName.ToLower().Contains(searchTerm) || d.PhoneNumber1.Contains(searchTerm)
            );
        }



    }
}
