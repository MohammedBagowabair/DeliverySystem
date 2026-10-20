using Application.Common.Models;
using Application.DTO.UserDtos;
using Domain.Common.Models;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<PagedList<User>> SearchUsersAsync(string searchTerm, int page, int pageSize);
        Task<User> Create(createUserDTO createUserDTO);
        Task<User> GetById(int id);
        Task<bool> Delete(int id);
        Task Update(UpdateUserDTO updateUserDTO);
        Task<IEnumerable<User>> GetAll();
        Task<JwtTokenModel> Authenticate(UserLoginDto user);
        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        PermissionCatalogDTO GetPermissionCatalog();
        Task<UpdateUserPermissionsResultDTO> UpdatePermissionsAsync(int userId, UpdateUserPermissionsDTO dto);
    }
}
