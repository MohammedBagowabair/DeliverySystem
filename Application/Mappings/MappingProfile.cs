using Application.DTO;
using AutoMapper;
using Domain.Entities;
using Domain.Common.Models; // Add this namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Application.DTO.OrderDtos;
using Application.DTO.CustomerDtos;
using Application.DTO.DriverDtos;
using Application.DTO.UserDtos;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<BaseEntity, BaseEntityDTO>().ReverseMap();
            CreateMap<Person, PersonDTO>().ReverseMap();
            CreateMap<User, UserDTO>().ReverseMap();

            CreateMap<Driver, DriverDTO>().ReverseMap();
            CreateMap<Customer, CustomerDTO>().ReverseMap();

            CreateMap<Customer, CreateUpdateCustomerDTO>().ReverseMap();
            CreateMap<Driver, CreateUpdateDriverDTO>().ReverseMap();

            CreateMap<Order, OrderDTO>().ReverseMap();
            CreateMap<Order, CreateOrderDTO>().ReverseMap();
            CreateMap<Order, UpdateOrderDTO>().ReverseMap();
            CreateMap<Order, CreateUpdateOrderDTO>().ReverseMap();
            CreateMap<Customer, CustomerOrderPagedDTO>().ReverseMap();
            CreateMap<Driver, DriverPagedDTO>().ReverseMap();



            // Add generic mapping for PagedList
            CreateMap(typeof(PagedList<>), typeof(PagedList<>))
                .ConvertUsing(typeof(PagedListConverter<,>));
        }
    }

    // Custom converter for PagedList
    public class PagedListConverter<TSource, TDestination>
        : ITypeConverter<PagedList<TSource>, PagedList<TDestination>>
    {
        private readonly IMapper _mapper;

        public PagedListConverter(IMapper mapper)
        {
            _mapper = mapper;
        }

        public PagedList<TDestination> Convert(
            PagedList<TSource> source,
            PagedList<TDestination> destination,
            ResolutionContext context)
        {
            var mappedEntities = _mapper.Map<List<TDestination>>(source.Entities);
            return new PagedList<TDestination>(
                source.TotalCount,
                mappedEntities,
                source.CurrentPage,
                source.PageSize);
        }
    }
}
