using AutoMapper;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.DTOs.Cart;
using MyWarehouse.Common.DTOs.Orders;
using MyWarehouse.Common.DTOs.Users;
using MyWarehouse.Data.Models;

namespace MyWarehouse.Common.ProfileData;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Categories, CategoryDTO>().ReverseMap();

        CreateMap<Cities, CityDTO>().ReverseMap();

        CreateMap<OrderDetails, OrderDetailDTO>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ReverseMap();

        CreateMap<Orders, OrderDTO>()
            .ForMember(dest => dest.StatusDescription, opt => opt.MapFrom(src => src.Status.Description))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails))
            .ForMember(dest => dest.AddressStreet, opt => opt.MapFrom(src => src.Address != null ? src.Address.Street : null))
            .ForMember(dest => dest.AddressCityName, opt => opt.MapFrom(src => src.Address != null ? src.Address.City.Name : null))
            .ForMember(dest => dest.IdAddress, opt => opt.MapFrom(src => src.IdAddress));

        CreateMap<OrderDTO, Orders>();

        CreateMap<Products, ProductDTO>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : "Sconosciuto"));
        CreateMap<ProductDTO, Products>();

        CreateMap<Suppliers, SupplierDTO>()
            .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.Name));
        CreateMap<SupplierDTO, Suppliers>()
            .ForMember(dest => dest.City, opt => opt.Ignore());

        CreateMap<Users, UserDTO>()
            .ForMember(dest => dest.Roles, opt =>
                opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()));
        CreateMap<RegisterDTO, Users>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        CreateMap<LoginDTO, Users>().ReverseMap();
        CreateMap<Roles, RoleDTO>().ReverseMap();

        CreateMap<CartItems, CartItemDTO>()
            .ForMember(dest => dest.IdProduct, opt => opt.MapFrom(src => src.IdProduct))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
            .ReverseMap();

        CreateMap<Carts, CartDTO>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ReverseMap();

        CreateMap<Addresses, AddressDTO>()
            .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.Name))
            .ReverseMap()
            .ForMember(dest => dest.City, opt => opt.Ignore());
    }
}