using Application.Common.Models;
using Application.DTO;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
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
        public async Task<ApiResultModel<IEnumerable<DriverDTO>>> GetAllAsync()
        {
            try
            {
                var results = _mapper.Map<IEnumerable<DriverDTO>>(await _service.GetAll());
                return new ApiResultModel<IEnumerable<DriverDTO>>(results);
            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<IEnumerable<DriverDTO>>(ex.Code, ex.Message, []);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "");
                return new ApiResultModel<IEnumerable<DriverDTO>>(500, ex.Message, []);
            }
            
        }

        [HttpGet]
        public async Task<ApiResultModel<DriverDTO>> GetAsync(int id)
        {
            try
            {
                var driver = await _service.GetById(id);
                var result = _mapper.Map<DriverDTO>(driver);
                return new ApiResultModel<DriverDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<DriverDTO>(ex.Code, ex.Message, null);
            }
            catch (Exception ex) 
            {
                return new ApiResultModel<DriverDTO>(500, ex.Message, null);

            }

        }

        [HttpPost]
        public async Task<ApiResultModel<DriverDTO>> AddAsync(DriverDTO driverDto)
        {
            try
            {
                var driver = await _service.Create(_mapper.Map<Driver>(driverDto));

                var result = _mapper.Map<DriverDTO>(driver);

                return new ApiResultModel<DriverDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<DriverDTO>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<DriverDTO>(500, ex.Message, null);
            }

        }

        [HttpDelete]
        public async Task<ApiResultModel<bool>> DeleteAsync(int id)
        {
            try
            {
                var result = await _service.Delete(id);
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


        [HttpPut]
        public async Task<ApiResultModel<bool>> UpdateAsync(DriverDTO driverDTO)
        {
            try
            {
                await _service.Update(_mapper.Map<Driver>(driverDTO));
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
    }
}
