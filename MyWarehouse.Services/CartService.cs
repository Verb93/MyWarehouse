using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Common.DTOs.Cart;
using MyWarehouse.Common.DTOs.Orders;
using MyWarehouse.Common.Response;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.SecurityInterface;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;

    public CartService(
        ICartRepository cartRepository,
        IAddressRepository addressRepository,
        IMapper mapper,
        IAuthorizationService authorizationService)
    {
        _cartRepository = cartRepository;
        _addressRepository = addressRepository;
        _mapper = mapper;
        _authorizationService = authorizationService;
    }

    // restituisce il carrello dell’utente loggato
    public async Task<ResponseBase<CartDTO>> GetCartAsync()
    {
        var response = new ResponseBase<CartDTO>();
        var userId = _authorizationService.GetCurrentUserId();

        var cartItems = await _cartRepository.GetCartItemsWithProductsAsync(userId);
        var cartDto = new CartDTO
        {
            Id = cartItems.FirstOrDefault()?.IdCart ?? 0,
            Items = _mapper.Map<List<CartItemDTO>>(cartItems)
        };

        response = ResponseBase<CartDTO>.Success(cartDto);
        return response;
    }

    // aggiunge o aggiorna un prodotto nel carrello
    public async Task<ResponseBase<bool>> AddToCartAsync(AddToCartRequest request)
    {
        var response = new ResponseBase<bool>();
        var userId = _authorizationService.GetCurrentUserId();

        if (request.Quantity <= 0)
        {
            response = ResponseBase<bool>.Fail("La quantità deve essere maggiore di 0.", ErrorCode.ValidationError);
        }
        else
        {
            await _cartRepository.AddOrUpdateItemAsync(userId, request.IdProduct, request.Quantity);
            response = ResponseBase<bool>.Success(true);
        }

        return response;
    }

    // rimuove un prodotto dal carrello
    public async Task<ResponseBase<bool>> RemoveFromCartAsync(int productId)
    {
        var response = new ResponseBase<bool>();
        var userId = _authorizationService.GetCurrentUserId();

        await _cartRepository.RemoveItemAsync(userId, productId);
        response = ResponseBase<bool>.Success(true);

        return response;
    }

    // svuota completamente il carrello
    public async Task<ResponseBase<bool>> ClearCartAsync()
    {
        var response = new ResponseBase<bool>();
        var userId = _authorizationService.GetCurrentUserId();

        await _cartRepository.ClearCartAsync(userId);
        response = ResponseBase<bool>.Success(true);

        return response;
    }

    public async Task<ResponseBase<OrderPreviewDTO>> CheckoutAsync(int idAddress)
    {
        var response = new ResponseBase<OrderPreviewDTO>();
        var userId = _authorizationService.GetCurrentUserId();

        var address = await _addressRepository.GetByIdAsync(idAddress);

        if (address == null || address.IdUser != userId)
        {
            response = ResponseBase<OrderPreviewDTO>.Fail("Indirizzo non valido o non associato all'utente.", ErrorCode.Unauthorized);
        }
        else
        {
            var cartItems = await _cartRepository.GetCartItemsWithProductsAsync(userId);

            if (!cartItems.Any())
            {
                response = ResponseBase<OrderPreviewDTO>.Fail("Il carrello è vuoto.", ErrorCode.ValidationError);
            }
            else
            {
                var preview = new OrderPreviewDTO
                {
                    AddressStreet = address.Street,
                    CityName = address.City?.Name ?? "",
                    TotalPrice = cartItems.Sum(i => i.Quantity * i.Product.Price),
                    Items = cartItems.Select(i => new OrderDetailPreviewDTO
                    {
                        ProductName = i.Product.Name,
                        UnitPrice = i.Product.Price,
                        Quantity = i.Quantity
                    }).ToList()
                };

                response = ResponseBase<OrderPreviewDTO>.Success(preview);
            }
        }

        return response;
    }

}
