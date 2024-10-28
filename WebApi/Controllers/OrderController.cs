using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        public IOrderService _service;
        public OrderController(IOrderService service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task<Order> AddAsync(Order order)
        {

            var result = await _service.Create(order);
            return result;
        }
    }
}
