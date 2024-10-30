using Application.DTO;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _service;
        private readonly IMapper _mapper; 
        public DriverController( IDriverService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }


        [HttpGet("GetAll")]
        public async Task<IEnumerable<DriverDTO>> GetAllAsync()
        {
            var results = _mapper.Map<IEnumerable<DriverDTO>>(await _service.GetAll());
            return results;
        }

        [HttpGet]
        public async Task<DriverDTO> GetAsync(int id)
        {
            var driver = await _service.GetById(id);
            var result = _mapper.Map<DriverDTO>(driver);
            return result;
        }

        [HttpPost]
        public async Task<DriverDTO> AddAsync(DriverDTO driverDto)
        {
            var driver = await _service.Create(_mapper.Map<Driver>(driverDto));
            
            return _mapper.Map<DriverDTO>(driver);
        }

        [HttpDelete]
        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _service.Delete(id);
            return result;
        }


        [HttpPut]
        public async Task<bool> UpdateAsync(DriverDTO driverDTO)
        {
            await _service.Update(_mapper.Map<Driver>(driverDTO));
            return true;
        }
    }
}
