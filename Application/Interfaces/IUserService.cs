using Application.Common.Models;
using Application.DTO.UserDtos;
using Domain.Common.Models;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserService
    {
        // Legacy LoginDto removed; use Authenticate(UserLoginDto)
        Task<PagedList<User>> SearchUsersAsync(string searchTerm, int page, int pageSize);
        Task<User> Create(createUserDTO createUserDTO);
        Task<User> GetById(int id);
        Task<bool> Delete(int id);
        Task Update(UpdateUserDTO updateUserDTO);
        Task<IEnumerable<User>> GetAll();
        public Task<JwtTokenModel> Authenticate(UserLoginDto user);




    }
}
