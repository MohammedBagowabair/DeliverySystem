using Application.Common.Models;
using Application.DTO.CustomerDtos;
using Application.DTO.DriverDtos;
using Application.DTO.UserDtos;
using Application.Interfaces;
using Application.Models;
using AutoMapper;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppPermissions.Operations)]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IMapper _mapper;
        public UserController(IUserService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("StaffList")]
        [Authorize(Roles = AppPermissions.Operations)]
        public async Task<ApiResultModel<IEnumerable<UserListItemDTO>>> GetStaffListAsync()
        {
            try
            {
                var users = await _service.GetAll();
                var result = _mapper.Map<IEnumerable<UserListItemDTO>>(users);
                return new ApiResultModel<IEnumerable<UserListItemDTO>>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<IEnumerable<UserListItemDTO>>(ex.Code, ex.Message, []);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<IEnumerable<UserListItemDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, []);
            }
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = AppPermissions.AdminOnly)]
        public async Task<ApiResultModel<IEnumerable<UpdateUserDTO>>> GetAllAsync()
        {
            try
            {
                var results = _mapper.Map<IEnumerable<UpdateUserDTO>>(await _service.GetAll());
                return new ApiResultModel<IEnumerable<UpdateUserDTO>>(results);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<IEnumerable<UpdateUserDTO>>(ex.Code, ex.Message, []);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<IEnumerable<UpdateUserDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, []);
            }

        }


        [HttpGet("SearchUsers")]
        [Authorize(Roles = AppPermissions.AdminOnly)]
        public async Task<ApiResultModel<PagedList<UserDTO>>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 10)
        {
            try
            {
                var results = await _service.SearchUsersAsync(searchTerm, page, pageSize);
                var mappedResults = _mapper.Map<PagedList<UserDTO>>(results);
                return new ApiResultModel<PagedList<UserDTO>>(mappedResults);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<PagedList<UserDTO>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<PagedList<UserDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }
        }

        [HttpPost("CreateUserAsync")]
        [Authorize(Roles = AppPermissions.AdminOnly)]
        public async Task<ApiResultModel<createUserDTO>> CreateUserAsync(createUserDTO createUserDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(createUserDTO.Password))
                    throw new DeliveryCoreException(ErrorCodes.USER_PASSWORD_IS_NULL_CODE);

                var user = await _service.Create(createUserDTO);
                var result = _mapper.Map<createUserDTO>(user);
                return new ApiResultModel<createUserDTO>(result);
            }


            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<createUserDTO>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<createUserDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }

        }

        [Route("/login")]
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var result = await _service.Authenticate(model);

                return Ok(new ResponseModel(result));
            }
            catch (DeliveryCoreException ex)
            {
                return BadRequest(new ResponseModel(ex.Code, ex.Message) { Content = ex.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE));
            }
        }



        //[Route("/login")]
        //[HttpPost, AllowAnonymous]
        //public async Task<IActionResult> Login([FromBody] UserLoginDto model)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            return BadRequest(ModelState);
        //        var result = await _service.Authenticate(model);

        //        return Ok(new ApiResultModel(result));
        //    }
        //    catch (DeliveryCoreException ex)
        //    {
        //        //return BadRequest(new ResponseModel(ex.Code, ex.Message) { Content = ex.Data });
        //        //return new ApiResultModel(ex.Code, ex.Message, ex.Message) { Content = ex.Data });

        //    }
        //    catch (Exception ex)
        //    {
        //        //_logger.LogWarning(ex, "")
        //        return new ApiResultModel<UserLoginDto>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
        //    }
        //}


        [HttpGet]
        [Authorize(Roles = AppPermissions.AdminOnly)]
        public async Task<ApiResultModel<UserDTO>> GetAsync(int id)
        {
            try
            {
                var result = _mapper.Map<UserDTO>(await _service.GetById(id));
                if (result is null)
                    return new ApiResultModel<UserDTO>(ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE, "السجل غير موجود", null);
                return new ApiResultModel<UserDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<UserDTO>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<UserDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }

        }

        [HttpDelete]
        [Authorize(Roles = AppPermissions.AdminOnly)]
        public async Task<ApiResultModel<bool>> DeleteAsync(int id)
        {
            try
            {
                var result = await _service.Delete(id);
                return new ApiResultModel<bool>(result);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<bool>(ex.Code, ex.Message, false);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<bool>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, false);
            }

        }


        [HttpPut]
        [Authorize(Roles = AppPermissions.AdminOnly)]
        public async Task<ApiResultModel<bool>> UpdateAsync(UpdateUserDTO updateUserDTO)
        {
            try
            {
                await _service.Update(updateUserDTO);
                return new ApiResultModel<bool>(true);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<bool>(ex.Code, ex.Message, false);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<bool>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, false);
            }

        }

        [HttpGet("permissions/catalog")]
        [Authorize(Roles = AppPermissions.AdminOnly)]
        public ApiResultModel<PermissionCatalogDTO> GetPermissionCatalog()
        {
            try
            {
                return new ApiResultModel<PermissionCatalogDTO>(_service.GetPermissionCatalog());
            }
            catch (Exception)
            {
                return new ApiResultModel<PermissionCatalogDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }
        }

        [HttpPut("{id:int}/permissions")]
        [Authorize(Roles = AppPermissions.AdminOnly)]
        public async Task<ApiResultModel<UpdateUserPermissionsResultDTO>> UpdatePermissionsAsync(int id, [FromBody] UpdateUserPermissionsDTO dto)
        {
            try
            {
                var result = await _service.UpdatePermissionsAsync(id, dto ?? new UpdateUserPermissionsDTO());
                return new ApiResultModel<UpdateUserPermissionsResultDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<UpdateUserPermissionsResultDTO>(ex.Code, ex.Message, null);
            }
            catch (Exception)
            {
                return new ApiResultModel<UpdateUserPermissionsResultDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }
        }


        [HttpPost("change-password")]
        [Authorize]
        public async Task<ApiResultModel<bool>> ChangePasswordAsync([FromBody] ChangePasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return new ApiResultModel<bool>(ErrorCodes.USER_PASSWORD_IS_NULL_CODE, "بيانات كلمة المرور غير صالحة", false);

                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                    return new ApiResultModel<bool>(ErrorCodes.USER_IS_NOT_AUTHORIZED_CODE, "غير مصرح", false);

                await _service.ChangePasswordAsync(userId, dto);
                return new ApiResultModel<bool>(true);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<bool>(ex.Code, ex.Message, false);
            }
            catch (Exception)
            {
                return new ApiResultModel<bool>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, false);
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ApiResultModel<bool>> ForgotPasswordAsync([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                await _service.ForgotPasswordAsync(dto ?? new ForgotPasswordDto());
                return new ApiResultModel<bool>(0, "إذا كان البريد مسجلاً لدينا، ستصلك تعليمات إعادة التعيين.", true);
            }
            catch (Exception)
            {
                return new ApiResultModel<bool>(0, "إذا كان البريد مسجلاً لدينا، ستصلك تعليمات إعادة التعيين.", true);
            }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ApiResultModel<bool>> ResetPasswordAsync([FromBody] ResetPasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return new ApiResultModel<bool>(ErrorCodes.USER_PASSWORD_IS_NULL_CODE, "بيانات إعادة التعيين غير صالحة", false);

                await _service.ResetPasswordAsync(dto);
                return new ApiResultModel<bool>(0, "تم تحديث كلمة المرور بنجاح", true);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<bool>(ex.Code, ex.Message, false);
            }
            catch (Exception)
            {
                return new ApiResultModel<bool>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, false);
            }
        }
    }
}
