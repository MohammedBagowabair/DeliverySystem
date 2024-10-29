using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDriverService
    {
        Task<bool> Delete(int id);

        Task<Driver> Create(Driver driver);

        Task Update(Driver driver);

        Task<IEnumerable<Driver>> GetAll();

        Task<Driver> GetById(int id);



    }
}
