using Application.DTO;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        public IOrderService _service;
        public IMapper _mapper;
        public OrderController(IOrderService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        [HttpGet("GetAll")]
        public async Task<IEnumerable<OrderDTO>> GetAllAsync()
        {

            var results = _mapper.Map<IEnumerable<OrderDTO>>(await _service.GetAll());


            return results;
        }

        [HttpGet]
        public async Task<OrderDTO> GetAsync(int id)
        {

            var order = await _service.GetById(id);
            var result = _mapper.Map<OrderDTO>(order);

            return result;
        }

        [HttpPost]
        public async Task<OrderDTO> AddAsync(OrderDTO orderDto)
        {

            var order = await _service.Create(_mapper.Map<Order>(orderDto));
            var result = _mapper.Map<OrderDTO>(order);


            return result;
        }
        [HttpDelete]
        public async Task<bool> DeleteAsync(int id)
        {

            var result = await _service.Delete(id);
            return result;
        }
        [HttpPut]
        public async Task<bool> UpdateAsync(OrderDTO orderDto)
        {

            await _service.Update(_mapper.Map<Order>(orderDto));
            return true;
        }
    }
}
