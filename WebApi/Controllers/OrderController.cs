using Application.Common.Models;
using Application.DTO;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
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
        public async Task<ApiResultModel<IEnumerable<OrderDTO>>> GetAllAsync()
        {
            try
            {

                var results = _mapper.Map<IEnumerable<OrderDTO>>(await _service.GetAll());


                return new ApiResultModel<IEnumerable<OrderDTO>>(results);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<IEnumerable<OrderDTO>>(ex.Code, ex.Message, []);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "");
                return new ApiResultModel<IEnumerable<OrderDTO>>(500, ex.Message, []);
            }
        }

        [HttpGet]
        public async Task<ApiResultModel<OrderDTO>> GetAsync(int id)
        {
            try
            {
                var order = await _service.GetById(id);
                var result = _mapper.Map<OrderDTO>(order);

                return new ApiResultModel<OrderDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<OrderDTO>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<OrderDTO>(500, ex.Message, null);
            }

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
