using Application.Commands.Driver;
using Application.Common.Models;
using Application.DTO.CustomerDtos;
using Application.DTO.DriverDtos;
using Application.Interfaces;
using Application.Mappings;
using Application.Queries.Driver;
using AutoMapper;
using Domain.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _service;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public DriverController(IDriverService service, IMapper mapper, IMediator mediator)
        {
            _service = service;
            _mapper = mapper;
            _mediator = mediator;
        }

        [HttpGet("GetAll")]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<IEnumerable<DriverDTO>>> GetAllAsync()
        {
            try
            {
                GetAllDriversQuery getAllDriversQuery = new GetAllDriversQuery();
                var result = await _mediator.Send(getAllDriversQuery);
                var resultDTO = _mapper.Map<IEnumerable<DriverDTO>>(result);
                return new ApiResultModel<IEnumerable<DriverDTO>>(resultDTO);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<IEnumerable<DriverDTO>>(ex.Code, ex.Message, []);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<IEnumerable<DriverDTO>>(500, ex.Message, []);
            }

        }

        [HttpGet]
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<DriverDTO>> GetAsync(int id)
        {
            try
            {
                GetDriverByIdQuery getDriverByIdQuery = new GetDriverByIdQuery(id);
                var result = await _mediator.Send(getDriverByIdQuery);

                var resulDtot = _mapper.Map<DriverDTO>(result);
                return new ApiResultModel<DriverDTO>(resulDtot);
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
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<CreateUpdateDriverDTO>> AddAsync(CreateUpdateDriverDTO createDriverDTO)
        {
            try
            {
                var driver = _mapper.Map<Driver>(createDriverDTO);
                CreateDriverCommand createDriverCommand = new (driver);

                var result = await _mediator.Send(createDriverCommand);
                var resultDriver= _mapper.Map<CreateUpdateDriverDTO>(result);

                return new ApiResultModel<CreateUpdateDriverDTO>(resultDriver);
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
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<bool>> DeleteAsync(int id)
        {
            try
            {
                DeleteDriverCommand deleteDriverCommand = new (id);
                var result = await _mediator.Send(deleteDriverCommand);
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
        //[Authorize(Roles = Roles.Staff + "," + Roles.Admin)]
        public async Task<ApiResultModel<bool>> UpdateAsync(CreateUpdateDriverDTO createUpdateDriverDTO)
        {
            try
            {
                var driver = _mapper.Map<Driver>(createUpdateDriverDTO);
                UpdateDriverCommand command = new (driver);
               await  _mediator.Send(command);
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



        [HttpGet("GetDriversPaged")]
        public async Task<ApiResultModel<PagedList<Driver>>> GetDriversPaged(int page = 1, int pageSize = 10)
        {
            try
            {
                GetAllPagedDriversQuery getAllPagedDriversQuery = new (page, pageSize);
                var result =await _mediator.Send(getAllPagedDriversQuery);

                return new ApiResultModel<PagedList<Driver>>(result);

            }
            catch (DeliveryCoreException ex)
            {
                //  _logger.LogWarning(ex, "");
                return new ApiResultModel<PagedList<Driver>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "")
                return new ApiResultModel<PagedList<Driver>>(500, ex.Message, null);
            }

        }

        [HttpGet("SearchDrivers")]
        public async Task<ApiResultModel<PagedList<DriverDTO>>> SearchDriversAsync(string searchTerm, int page = 1, int pageSize = 10)
        {
            try
            {
                GetAllSearchPagedDriversQuery getAllSearchPagedDriversQuery =new (searchTerm, page,pageSize);
                var result = await _mediator.Send(getAllSearchPagedDriversQuery);

                var mappedResults = _mapper.MapPagedList<Driver, DriverDTO>(result);
                return new ApiResultModel<PagedList<DriverDTO>>(mappedResults);
            }
            catch (DeliveryCoreException ex)
            {
                return new ApiResultModel<PagedList<DriverDTO>>(ex.Code, ex.Message, null);
            }
            catch (Exception ex)
            {
                return new ApiResultModel<PagedList<DriverDTO>>(500, ex.Message, null);
            }
        }



    }
}
