using Application.DTO;
using AutoMapper;
using Domain.Entities;
using Domain.Common.Models;
using Domain.Constants;
using Application.DTO.OrderDtos;
using Application.DTO.CompanySettingsDtos;
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

            CreateMap<Driver, DriverDTO>().ReverseMap();
            CreateMap<Customer, CustomerDTO>().ReverseMap();

            CreateMap<Customer, CreateUpdateCustomerDTO>().ReverseMap();
            CreateMap<Driver, CreateUpdateDriverDTO>().ReverseMap();

            CreateMap<Order, OrderDTO>()
                .ForMember(d => d.CreatedByUserName, o => o.MapFrom(s => s.CreatedByUser != null ? s.CreatedByUser.FullName : null))
                .ForMember(d => d.DriverName, o => o.MapFrom(s =>
                    !string.IsNullOrWhiteSpace(s.DriverName) ? s.DriverName : (s.Driver != null ? s.Driver.FullName : string.Empty)))
                .ForMember(d => d.CustomerName, o => o.MapFrom(s =>
                    !string.IsNullOrWhiteSpace(s.CustomerName) ? s.CustomerName : (s.Customer != null ? s.Customer.FullName : string.Empty)))
                .ReverseMap()
                .ForMember(d => d.CreatedByUser, o => o.Ignore())
                .ForMember(d => d.Driver, o => o.Ignore())
                .ForMember(d => d.Customer, o => o.Ignore());
            CreateMap<Order, CreateOrderDTO>().ReverseMap();
            CreateMap<Order, UpdateOrderDTO>().ReverseMap();
            CreateMap<Order, CreateUpdateOrderDTO>().ReverseMap();
            CreateMap<User, UserListItemDTO>()
                .ForMember(d => d.Permissions, o => o.MapFrom(s =>
                    string.Equals(s.Role, Roles.Admin, StringComparison.OrdinalIgnoreCase)
                        ? StaffPermissions.All.ToList()
                        : StaffPermissions.Parse(s.Permissions)));
            CreateMap<Customer, CustomerOrderPagedDTO>().ReverseMap();
            CreateMap<Driver, DriverPagedDTO>().ReverseMap();

            CreateMap<User, UserDTO>()
                .ForMember(d => d.Permissions, o => o.MapFrom(s => StaffPermissions.Parse(s.Permissions)))
                .ReverseMap()
                .ForMember(d => d.Permissions, o => o.MapFrom(s => StaffPermissions.Serialize(s.Permissions)));

            CreateMap<User, createUserDTO>()
                .ForMember(d => d.Permissions, o => o.MapFrom(s => StaffPermissions.Parse(s.Permissions)))
                .ForMember(d => d.Password, o => o.Ignore())
                .ForMember(d => d.ConfirmPassword, o => o.Ignore())
                .ReverseMap()
                .ForMember(d => d.Permissions, o => o.MapFrom(s => StaffPermissions.NormalizeForRole(s.Role, s.Permissions)))
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.PasswordSalt, o => o.Ignore());

            CreateMap<User, UpdateUserDTO>()
                .ForMember(d => d.Permissions, o => o.MapFrom(s => StaffPermissions.Parse(s.Permissions)))
                .ReverseMap()
                .ForMember(d => d.Permissions, o => o.MapFrom(s => StaffPermissions.NormalizeForRole(s.Role, s.Permissions)))
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.PasswordSalt, o => o.Ignore());

            CreateMap<CompanySettings, CompanySettingsDTO>().ReverseMap();

            CreateMap(typeof(PagedList<>), typeof(PagedList<>))
                .ConvertUsing(typeof(PagedListConverter<,>));
        }
    }

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
