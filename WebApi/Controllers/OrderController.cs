using Application.Common.Models;
using Application.DTO.OrderDtos;
using Application.Interfaces;
using AutoMapper;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
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
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
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
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
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
        public async Task<ApiResultModel<OrderDTO>> AddAsync([FromBody] CreateOrderDTO createOrderDTO)
        {
            try
            {
                var order = _mapper.Map<Order>(createOrderDTO);

                // Create the order
                var createdOrder = await _service.Create(order);

                // Map back to DTO for the response
                var result = _mapper.Map<OrderDTO>(createdOrder);
                return new ApiResultModel<OrderDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<OrderDTO>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<OrderDTO>(500, ex.Message, null);
            }
        }

        [HttpDelete]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
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
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<bool>> UpdateAsync(UpdateOrderDTO updateOrderDTO)
        {
            try
            {
                await _service.Update(_mapper.Map<Order>(updateOrderDTO));
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


        [HttpGet("GetOrdersPaged")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> GetOrdersPaged(int page = 1, int pageSize = 10)
        {
            try
            {

                return new ApiResultModel<PagedList<OrderDTO>>(_mapper.Map < PagedList < OrderDTO > >(await _service.GetAllPagedAsync(page, pageSize)));

            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<PagedList<OrderDTO>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<PagedList<OrderDTO>>(500, ex.Message, null);
            }

        }

        [HttpGet("SearchOrders")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> SearchOrdersAsync(string searchTerm, int page = 1, int pageSize = 10)
        {
            try
            {
                var results = await _service.SearchOrdersAsync(searchTerm, page, pageSize);
                var mappedResults = _mapper.Map<PagedList<OrderDTO>>(results);
                return new ApiResultModel<PagedList<OrderDTO>>(mappedResults);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(500, ex.Message, null);
            }
        }


        [HttpGet("GetDriverOrders")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> GetDriverOrders(int driverId,string searchTerm, int page = 1, int pageSize = 10, DateTime? startDate=null,
    DateTime? endDate=null)
        {
            try
            {
                var results = await _service.GetAllOrdersByDriverId(driverId,searchTerm, page, pageSize,startDate,endDate);
                var mappedResults = _mapper.Map<PagedList<OrderDTO>>(results);
                return new ApiResultModel<PagedList<OrderDTO>>(mappedResults);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<PagedList<OrderDTO>>(500, ex.Message, null);
            }
        }
    }
}
