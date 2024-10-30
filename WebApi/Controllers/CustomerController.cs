using Application.DTO;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;
        private readonly IMapper _mapper;
        public CustomerController(ICustomerService service,IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }


        [HttpGet("GetAll")]
        public async Task<IEnumerable<CustomerDTO>> GetAllAsync()
        {
            var results= _mapper.Map<IEnumerable<CustomerDTO>>(await _service.GetAll());
            return results;
        }

        [HttpGet]
        public async Task<CustomerDTO> GetAsync(int id)
        {
            var result = _mapper.Map<CustomerDTO>(await _service.GetById(id));
            return result;
        }

        [HttpPost]
        public async Task<CustomerDTO> AddAsync( CustomerDTO customerDTO)
        {
            var customer = await _service.Create( _mapper.Map<Customer>(customerDTO));
            return _mapper.Map<CustomerDTO>(customer);
        }

        [HttpPut]
        public async Task<bool> UpdateAsync(CustomerDTO customerDTO)
        {
            await _service.Update(_mapper.Map<Customer>(customerDTO));
            return true;
        }

        [HttpDelete]
        public async Task<bool> DeleteAsync(int id) 
        {
          var result = await _service.Delete(id);
            return result;
        }
    }
}
