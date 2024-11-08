using Application.Common.Models;
using Application.DTO;
using Application.Interfaces;
using AutoMapper;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = Roles.Staff + "," + Roles.Admin)]

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
        [Authorize(Roles = Roles.Staff + "," + Roles.Admin)]

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
        [Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<CreateUpdateDriverDTO>> AddAsync(CreateUpdateDriverDTO createDriverDTO)
        {
            try
            {
                var driver = await _service.Create(_mapper.Map<Driver>(createDriverDTO));

                var result = _mapper.Map<CreateUpdateDriverDTO>(driver);

                return new ApiResultModel<CreateUpdateDriverDTO>(result);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<CreateUpdateDriverDTO>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<CreateUpdateDriverDTO>(500, ex.Message, null);
            }

        }

        [HttpDelete]
        [Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
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
        [Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<bool>> UpdateAsync(CreateUpdateDriverDTO createUpdateDriverDTO)
        {
            try
            {
                await _service.Update(_mapper.Map<Driver>(createUpdateDriverDTO));
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
