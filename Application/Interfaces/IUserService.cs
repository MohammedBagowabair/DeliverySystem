using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<User> Create(User user);

        Task Update(User user);

        Task<bool> Delete(int id);

        Task<User> GetById(int id);

        Task<IEnumerable<User>> GetAll();
    }
}
