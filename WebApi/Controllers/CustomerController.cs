using Application.Commands.Customer;
using Application.Commands.Order;
using Application.Common.Models;
using Application.DTO.CustomerDtos;
using Application.Handlers.Customer.CommandHandler;
using Application.Interfaces;
using Application.Mappings;
using Application.Queries.Customer;
using AutoMapper;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IMediator _mediator;

        public CustomerController(ICustomerService service,IMapper mapper, IMediator mediator)
        {
            _service = service;
            _mapper = mapper;
            _mediator = mediator;
        }


        [HttpGet("GetAll")]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<IEnumerable<CustomerDTO>>> GetAllAsync()
        {
            try
            {
                GetAllCustomersQuery getAllCustomersQuery = new GetAllCustomersQuery();
                var result = await _mediator.Send(getAllCustomersQuery);

                var resultDto = _mapper.Map<IEnumerable<CustomerDTO>>(await _service.GetAll());
                return new ApiResultModel<IEnumerable<CustomerDTO>>(resultDto);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<IEnumerable<CustomerDTO>>(ex.Code, ex.Message, []);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<IEnumerable<CustomerDTO>>(500, ex.Message, []);
            }
            
        }

        [HttpGet]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<CustomerDTO>> GetAsync(int id)
        {
            try
            {
                GetCustomerByIdQuery getCustomerByIdQuery = new (id);
                var result = await _mediator.Send(getCustomerByIdQuery);

                var resultDto = _mapper.Map<CustomerDTO>(result);
                return new ApiResultModel<CustomerDTO>(resultDto);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<CustomerDTO>(ex.Code, ex.Message,null);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<CustomerDTO>(500, ex.Message, null);
            }

        }

        [HttpPost]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<CreateUpdateCustomerDTO>> AddAsync(CreateUpdateCustomerDTO createCustomerDTO)
        {
           
            try
            {
                var customer = _mapper.Map<Customer>(createCustomerDTO);
                CreateCustomerCommand createCustomerCommand = new(customer);
                var result = await _mediator.Send(createCustomerCommand);

                var resultCustomer= _mapper.Map<CreateUpdateCustomerDTO>(result);

                return new ApiResultModel<CreateUpdateCustomerDTO>(resultCustomer);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<CreateUpdateCustomerDTO>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<CreateUpdateCustomerDTO>(500, ex.Message, null);
            }
        }

        [HttpPut]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<bool>> UpdateAsync(CreateUpdateCustomerDTO createUpdateCustomerDTO)
        {
            try
            {
                var customer =  _mapper.Map<Customer>(createUpdateCustomerDTO);

                UpdateCustomerCommand updateCustomerCommand = new (customer);

                var result =   _mediator.Send(updateCustomerCommand);

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

        [HttpDelete]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<bool>> DeleteAsync(int id) 
        {
            try
            {
                DeleteCustomerCommand deleteCustomerCommand = new (id);
                var result= await _mediator.Send(deleteCustomerCommand);
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

        //[HttpGet("GetCustomersPaged")]
        //public async Task<ApiResultModel<PagedList<Customer>>> GetCustomersPaged(int page = 1, int pageSize = 10)
        //{
        //    try
        //    {
        //        GetAllPagedCustomersQuery getAllPagedCustomers = new(page,pageSize);
        //        var result = await _mediator.Send(getAllPagedCustomers);

        //        return new ApiResultModel<PagedList<Customer>>(result);

        //    }
        //    catch (DeliveryCoreException ex)
        //    {
        //        //  _logger.LogWarning(ex, "");
        //        return new ApiResultModel<PagedList<Customer>>(ex.Code, ex.Message, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        //_logger.LogWarning(ex, "")
        //        return new ApiResultModel<PagedList<Customer>>(500, ex.Message, null);
        //    }

        //}

        [HttpGet("SearchCustomers")]
        public async Task<ApiResultModel<PagedList<CustomerDTO>>> SearchCustomersAsync(string searchTerm, int page = 1, int pageSize = 10)
        {
            try
            {
                GetAllSearchPagedCustomersQuery getAllSearchPagedCustomersQuery = new(searchTerm, page, pageSize);
                var result =await _mediator.Send(getAllSearchPagedCustomersQuery);

                var mappedResults = _mapper.MapPagedList<Customer,CustomerDTO>(result);
                return new ApiResultModel<PagedList<CustomerDTO>>(mappedResults);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<PagedList<CustomerDTO>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<PagedList<CustomerDTO>>(500, ex.Message, null);
            }
        }
    }
}
