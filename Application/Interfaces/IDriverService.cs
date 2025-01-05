using Domain.Common.Models;
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
        Task<IEnumerable<Driver>> GetAll();
        Task<Driver> GetById(int id);
        Task<Driver> Create(Driver driver);
        Task Update(Driver driver);
        Task<bool> Delete(int id);

        Task<int> CountAsync();

        Task<PagedList<Driver>> GetAllPagedAsync(int page, int PageSize);

        Task<PagedList<Driver>> SearchDriversAsync(string searchTerm, int page, int pageSize);


    }
}
