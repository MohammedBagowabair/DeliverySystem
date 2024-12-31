using Application.Common.Models;
using Application.DTO.AccountDtos;
using Application.DTO.UserDtos;
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
        Task<User> Register(UserDTO userDTO);
        Task<JwtTokenModel> Login(LoginDto loginDto);

        Task Update(User user);

        Task<bool> Delete(int id);

        Task<User> GetById(int id);

        Task<IEnumerable<User>> GetAll();
    }
}
