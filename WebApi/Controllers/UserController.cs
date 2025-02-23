using Application.Common.Models;
using Application.DTO.AccountDtos;
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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IMapper _mapper;
        public UserController(IUserService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]
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
                return new ApiResultModel<IEnumerable<UpdateUserDTO>>(500, ex.Message, []);
            }

        }


        [HttpGet("SearchUsers")]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
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
                return new ApiResultModel<PagedList<UserDTO>>(500, ex.Message, null);
            }
        }

        [HttpPost("CreateUserAsync")]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
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
                return new ApiResultModel<createUserDTO>(500, ex.Message, null);
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
                return StatusCode(500, new ResponseModel(500, ex.Message));
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
        //        return new ApiResultModel<UserLoginDto>(500, ex.Message, null);
        //    }
        //}


        [HttpGet]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<UserDTO>> GetAsync(int id)
        {
            try
            {
                var result = _mapper.Map<UserDTO>(await _service.GetById(id));
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
                return new ApiResultModel<UserDTO>(500, ex.Message, null);
            }

        }

        [HttpDelete]
        //[Authorize(Roles = Roles.Admin)]
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
                return new ApiResultModel<bool>(500, ex.Message, false);
            }

        }


        [HttpPut]
        //[Authorize(Roles = Roles.Admin)]
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
                return new ApiResultModel<bool>(500, ex.Message, false);
            }

        }


        //[HttpPost("Login")]
        //public async Task<ApiResultModel<JwtTokenModel>>Login(LoginDto loginDTO)
        //{
        //    try
        //    {
        //        var userLogin = await _service.Login(loginDTO);
        //        return new ApiResultModel<JwtTokenModel>(userLogin);    
        //    }
        //    catch (DeliveryCoreException ex)
        //    {
        //        //  _logger.LogWarning(ex, "");
        //        return new ApiResultModel<JwtTokenModel>(ex.Code, ex.Message, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        //_logger.LogWarning(ex, "")
        //        return new ApiResultModel<JwtTokenModel>(500, ex.Message, null);
        //    }
        //}




    }
}
