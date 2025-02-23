using Application.Common.Models;
using Application.DTO.AccountDtos;
using Application.DTO.UserDtos;
using Domain.Common.Models;
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
        //Task<JwtTokenModel> Login(LoginDto loginDto);
        Task<PagedList<User>> SearchUsersAsync(string searchTerm, int page, int pageSize);
        Task<User> Create(createUserDTO createUserDTO);
        Task<User> GetById(int id);
        Task<bool> Delete(int id);
        Task Update(UpdateUserDTO updateUserDTO);
        Task<IEnumerable<User>> GetAll();
        public Task<JwtTokenModel> Authenticate(UserLoginDto user);




    }
}
