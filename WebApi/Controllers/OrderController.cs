using Application.Common.Models;
using Application.DTO.OrderDtos;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Swashbuckle.AspNetCore.Annotations;
using static System.Net.Mime.MediaTypeNames;
using PdfSharp;
using MediatR;
using Application.Commands.Order;
using Application.Queries.Order;
using Application.Commands.Customer;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        public IOrderService _service;
        public IMapper _mapper;
        private readonly IPdfGeneratorService _pdfGeneratorService;
        private readonly IMediator _mediator ;


        public OrderController(IMediator mediator,IOrderService service, IMapper mapper, IPdfGeneratorService pdfGeneratorService)
        {
            _mediator = mediator;
            _service = service;
            _mapper = mapper;
            _pdfGeneratorService = pdfGeneratorService;


        }

        [HttpGet("GetAll")]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<IEnumerable<OrderDTO>>> GetAllAsync()
        {
            try
            {
                GetAllOrdersQuery getAllOrdersQuery = new GetAllOrdersQuery();
                var result = await _mediator.Send(getAllOrdersQuery);

                var results = _mapper.Map<IEnumerable<OrderDTO>>(result);


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
                GetOrderByIdQuery getOrderByIdQuery = new(id);
                var result =await _mediator.Send(getOrderByIdQuery);

                var resultOrder =  _mapper.Map<OrderDTO>(result);
                return new ApiResultModel<OrderDTO>(resultOrder);
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

                CreateOrderCommand createOrderCommand = new CreateOrderCommand(order);

                var result = await _mediator.Send(createOrderCommand);
                // Create the order
                // Map back to DTO for the response
                var resultDto = _mapper.Map<OrderDTO>(result);
                return new ApiResultModel<OrderDTO>(resultDto);
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

                DeleteOrderCommand deleteCustomerCommand = new DeleteOrderCommand(id);
                var result = await _mediator.Send(deleteCustomerCommand);

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
                var order = _mapper.Map<Order>(updateOrderDTO);
                UpdateOrderCommand command = new UpdateOrderCommand(order);
                var result = _mediator.Send(command);
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

                return new ApiResultModel<PagedList<OrderDTO>>(_mapper.Map<PagedList<OrderDTO>>(await _service.GetAllPagedAsync(page, pageSize)));

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




        [HttpGet("GetDriverOrders")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> GetDriverOrders(int driverId, string searchTerm, int page = 1, int pageSize = 10, DateTime? startDate = null,DateTime? endDate = null)
        {
            try
            {
                var results = await _service.GetAllOrdersByDriverId(driverId, searchTerm, page, pageSize, startDate, endDate);
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


        [HttpGet("TodayOrders")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> TodayOrders(string searchTerm, int page = 1, int pageSize = 10)
        {
            try
            {
                var results = await _service.TodayOrdersAsync(searchTerm, page, pageSize);
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
        [HttpGet("GetLastWeekOrders")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> GetLastWeekOrders(string searchTerm, int page = 1, int pageSize = 10, DateTime? startDate = null,
    DateTime? endDate = null)
        {
            try
            {
                var results = await _service.GetLastWeekOrdersAsync(searchTerm, page, pageSize, startDate, endDate);
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

        [HttpGet("GetAllOrders")]
        public async Task<ApiResultModel<PagedList<OrderDTO>>> GetAllOrders(string searchTerm, int page = 1, int pageSize = 10,


            [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null)

        {
            try
            {
                var results = await _service.GetAllOrdersAsync(searchTerm, page, pageSize, startDate, endDate);
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


        [HttpGet("download-pdf")]
        public async Task<IActionResult> DownloadPdf()
        {
            try
            {
                // Fetch the orders from the service
                var pagedOrders = await _service.GetPDFLastWeekOrdersAsync();

                if (pagedOrders?.Entities == null || !pagedOrders.Entities.Any())
                {
                    // If no orders are found, return a "Not Found" response
                    return NotFound("No orders found.");
                }

                // Generate the PDF using the PdfGeneratorService
                var pdfBytes = _pdfGeneratorService.GenerateOrderPdf(pagedOrders.Entities);

                // Return the PDF as a file to the client
                return File(pdfBytes, "application/pdf", "LastWeekOrders.pdf");
            }
            catch (Exception ex)
            {
                // If any error occurs during PDF generation, return a server error response
                return StatusCode(500, new { message = "Error generating PDF", error = ex.Message });
            }
        }



    }
}
