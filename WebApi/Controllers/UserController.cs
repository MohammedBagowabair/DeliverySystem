using Application.Common.Models;
using Application.DTO;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
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
        public UserController(IUserService service,IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("GetAll")]
        public async Task<ApiResultModel<IEnumerable<UserDTO>>> GetAllAsync()
        {
            try
            {
                var results = _mapper.Map<IEnumerable<UserDTO>>(await _service.GetAll());
                return new ApiResultModel<IEnumerable<UserDTO>>(results);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<IEnumerable<UserDTO>>(ex.Code, ex.Message, []);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<IEnumerable<UserDTO>>(500, ex.Message, []);
            }
            
        }

        [HttpGet]
        public async Task<ApiResultModel<UserDTO>> GetAsync(int id)
        {
            try
            {
                var user = await _service.GetById(id);
                var result = _mapper.Map<UserDTO>(user);
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

        [HttpPost]
        public async Task<ApiResultModel<UserDTO>> AddAsync(UserDTO userDTO)
        {
            try
            {
                var user = await _service.Create(_mapper.Map<User>(userDTO));
                var result = _mapper.Map<UserDTO>(user);
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
        public async Task<ApiResultModel<bool>> UpdateAsync(UserDTO userDTO)
        {
            try
            {
                await _service.Update(_mapper.Map<User>(userDTO));
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

    }
}
