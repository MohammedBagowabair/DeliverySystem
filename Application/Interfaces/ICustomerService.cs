using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICustomerService
    {
        Task<Customer> Create(Customer customer);

        Task Update(Customer customer);

        Task<bool> Delete(int id);

        Task<Customer> GetById(int id);

        Task<IEnumerable<Customer>> GetAll();
    }
}
