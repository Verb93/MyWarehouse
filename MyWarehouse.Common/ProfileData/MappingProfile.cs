using AutoMapper;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Data.Models;

namespace MyWarehouse.Common.ProfileData;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Categories, CategoryDTO>().ReverseMap();
        CreateMap<Cities, CityDTO>().ReverseMap();
        CreateMap<OrderDetails, OrderDetailDTO>().ReverseMap();
        CreateMap<Orders, OrderDTO>()
            .ForMember(dest => dest.StatusDescription, opt => opt.MapFrom(src => src.Status.Description));
        CreateMap<OrderDTO, Orders>();
        CreateMap<Products, ProductDTO>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : "Sconosciuto"));
        CreateMap<ProductDTO, Products>();
        CreateMap<Suppliers, SupplierDTO>()
            .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.Name))
            .ReverseMap();
        CreateMap<OrderDetails, OrderDTO>();
        CreateMap<Users, UserDTO>();
        CreateMap<RegisterDTO, Users>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        CreateMap<LoginDTO, Users>().ReverseMap();

    }
}