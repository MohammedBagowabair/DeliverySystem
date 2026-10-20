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
    [Authorize(Roles = AppPermissions.CustomersAccess)]
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
        [Authorize(Roles = AppPermissions.CustomersAccess)]
        public async Task<ApiResultModel<IEnumerable<CustomerDTO>>> GetAllAsync()
        {
            try
            {
                var result = await _mediator.Send(new GetAllCustomersQuery());
                var resultDto = _mapper.Map<IEnumerable<CustomerDTO>>(result);
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
                return new ApiResultModel<IEnumerable<CustomerDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, []);
            }
            
        }

        [HttpGet]
        [Authorize(Roles = AppPermissions.CustomersAccess)]
        public async Task<ApiResultModel<CustomerDTO>> GetAsync(int id)
        {
            try
            {
                GetCustomerByIdQuery getCustomerByIdQuery = new (id);
                var result = await _mediator.Send(getCustomerByIdQuery);

                var resultDto = _mapper.Map<CustomerDTO>(result);
                if (resultDto is null)
                    return new ApiResultModel<CustomerDTO>(ErrorCodes.DATABASE_RECORD_NOT_FOUND_CODE, "السجل غير موجود", null);
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
                return new ApiResultModel<CustomerDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }

        }

        [HttpPost]
        [Authorize(Roles = AppPermissions.CustomersAccess)]
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
                return new ApiResultModel<CreateUpdateCustomerDTO>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }
        }

        [HttpPut]
        [Authorize(Roles = AppPermissions.CustomersAccess)]
        public async Task<ApiResultModel<bool>> UpdateAsync(CreateUpdateCustomerDTO createUpdateCustomerDTO)
        {
            try
            {
                var customer =  _mapper.Map<Customer>(createUpdateCustomerDTO);

                UpdateCustomerCommand updateCustomerCommand = new (customer);

                 await   _mediator.Send(updateCustomerCommand);

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
                return new ApiResultModel<bool>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, false);
            }
            
        }

        [HttpDelete]
        [Authorize(Roles = AppPermissions.CustomersAccess)]
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
                return new ApiResultModel<bool>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, false);
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
        //        return new ApiResultModel<PagedList<Customer>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
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
                return new ApiResultModel<PagedList<CustomerDTO>>(ErrorCodes.SYS_UNKNOWN_ERROR_CODE, ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE, null);
            }
        }
    }
}
