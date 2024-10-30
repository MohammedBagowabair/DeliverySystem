using Application.DTO;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
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
        public async Task<IEnumerable<UserDTO>> GetAllAsync()
        {
            var results = _mapper.Map<IEnumerable<UserDTO>>(await _service.GetAll());
            return results;
        }

        [HttpGet]
        public async Task<UserDTO> GetAsync(int id)
        {
            var user = await _service.GetById(id);
            var result = _mapper.Map<UserDTO>(user);
            return result;
        }

        [HttpPost]
        public async Task<UserDTO> AddAsync(UserDTO userDTO)
        {
            var user= await _service.Create(_mapper.Map<User>(userDTO));
            return _mapper.Map<UserDTO>(user);
        }

        [HttpDelete]
        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _service.Delete(id);
            return result;
        }

        [HttpPut]
        public async Task<bool> UpdateAsync(UserDTO userDTO)
        {
            await _service.Update(_mapper.Map<User>(userDTO));
            return true;
        }

    }
}
