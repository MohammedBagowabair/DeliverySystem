using Application.Common.Models;
using Application.DTO.CustomerDtos;
using Application.Interfaces;
using AutoMapper;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
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
        public CustomerController(ICustomerService service,IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }


        [HttpGet("GetAll")]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<IEnumerable<CustomerDTO>>> GetAllAsync()
        {
            try
            {
                var results = _mapper.Map<IEnumerable<CustomerDTO>>(await _service.GetAll());
                return new ApiResultModel<IEnumerable<CustomerDTO>>(results);
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
                var result = _mapper.Map<CustomerDTO>(await _service.GetById(id));
                return new ApiResultModel<CustomerDTO>(result);
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
                var customer = await _service.Create(_mapper.Map<Customer>(createCustomerDTO));

                var result= _mapper.Map<CreateUpdateCustomerDTO>(customer);

                return new ApiResultModel<CreateUpdateCustomerDTO>(result);
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
                await _service.Update(_mapper.Map<Customer>(createUpdateCustomerDTO));
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

        [HttpGet("GetCustomersPaged")]
        public async Task<ApiResultModel<PagedList<Customer>>> GetCustomersPaged(int page = 1, int pageSize = 10)
        {
            try
            {

                return new ApiResultModel<PagedList<Customer>>(await _service.GetAllPagedAsync(page, pageSize));

            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<PagedList<Customer>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<PagedList<Customer>>(500, ex.Message, null);
            }

        }

        [HttpGet("SearchCustomers")]
        public async Task<ApiResultModel<PagedList<CustomerDTO>>> SearchCustomersAsync(string searchTerm, int page = 1, int pageSize = 10)
        {
            try
            {
                var results = await _service.SearchCustomersAsync(searchTerm, page, pageSize);
                var mappedResults = _mapper.Map<PagedList<CustomerDTO>>(results);
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
