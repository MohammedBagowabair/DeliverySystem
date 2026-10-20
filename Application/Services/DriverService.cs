using Application.Interfaces;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

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
            if (driver.CommissionRate <= 0)
                driver.CommissionRate = Application.Helpers.RevenueSplit.DefaultDriverCommissionRate;

            ValidateCommissionRate(driver.CommissionRate);
            var driverInDb = (await _dbContext.GetAsync<Driver>(x => x.PhoneNumber1==driver.PhoneNumber1))?.FirstOrDefault();
            if (driverInDb != null)
            {
                throw new DeliveryCoreException(ErrorCodes.USER_ALREADY_EXISTS_CODE);
            }
            return await _dbContext.AddAsync<Driver>(driver);
        }
        public async Task Update(Driver driver)
        {
            ValidateCommissionRate(driver.CommissionRate);
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

        private static void ValidateCommissionRate(decimal rate)
        {
            if (rate <= 0 || rate > 1)
            {
                throw new DeliveryCoreException(
                    "نسبة عمولة الموصل يجب أن تكون بين 1% و 100%.",
                    ErrorCodes.DB_UNABLE_TO_UPDATE_RECORD_CODE,
                    null);
            }
        }
        public async Task<bool> Delete(int id)
        {
            var hasOrders = await _dbContext._dbContext.Set<Order>().AnyAsync(o => o.DriverId == id);
            if (hasOrders)
            {
                var driver = await _dbContext._dbContext.Set<Driver>().FirstOrDefaultAsync(d => d.Id == id)
                    ?? throw new DeliveryCoreException(ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE);

                if (driver.IsActive)
                {
                    driver.IsActive = false;
                    await _dbContext._dbContext.SaveChangesAsync();
                }

                // Soft-deactivate preserves order history (DriverName denormalized on orders).
                return true;
            }

            return await _dbContext.DeleteAsync<Driver>(id);
        }
 
        public async Task<int> CountAsync()
        {
            return await _dbContext.CountAsync<Driver>();
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
