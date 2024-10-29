using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserService : IUserService
    {
        public IApplicationDbContext _dbContext;
        public UserService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> Create(User user)
        {
            return await _dbContext.AddAsync<User>(user);
        }

        public async Task<bool> Delete(int id)
        {
            return await _dbContext.DeleteAsync<User>(id);
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _dbContext.GetAsync<User>();
        }

        public async Task<User> GetById(int id)
        {
            return (await _dbContext.GetAsync<User>(x=>x.Id == id))?.FirstOrDefault();
        }

        public async Task Update(User user)
        {
            await _dbContext.UpdateAsync<User>(user);
        }
    }
}
