using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Common.DTOs.Orders;
using MyWarehouse.Common.Response;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.SecurityInterface;
using MyWarehouse.Interfaces.ServiceInterfaces;
using System.Data;

namespace MyWarehouse.Services;

public class OrderService : GenericService<Orders, OrderDTO>, IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public OrderService(IOrderRepository repository, 
        IProductRepository productRepository, 
        ICartRepository cartRepository,
        IAddressRepository addressRepository,
        IAuthorizationService authorizationService, 
        IMapper mapper) : base(repository, mapper)
    {
        _repository = repository;
        _productRepository = productRepository;
        _cartRepository = cartRepository;
        _addressRepository = addressRepository;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    // all'inserimento di un nuovo ordine controllo:
    // che i prodotti esistano e abbiano quantità sufficiente
    // che il prezzo unitario venga copiato correttamente
    // che il prezzo totale venga calcolato correttamente
    public async Task<ResponseBase<bool>> CreateOrderAsync(CheckoutRequest dto)
    {
        var response = new ResponseBase<bool>();
        var userId = _authorizationService.GetCurrentUserId();

        var address = await _addressRepository.GetByIdAsync(dto.IdAddress);
        if (address == null || address.IdUser != userId)
        {
            response = ResponseBase<bool>.Fail("Indirizzo non valido o non associato all'utente.", ErrorCode.Unauthorized);
        }
        else
        {
            var cartItems = await _cartRepository.GetCartItemsWithProductsAsync(userId);
            if (!cartItems.Any())
            {
                response = ResponseBase<bool>.Fail("Il carrello è vuoto.", ErrorCode.ValidationError);
            }
            else
            {
                bool hasErrors = false;

                // Raggruppa per fornitore
                var grouped = cartItems.GroupBy(i => i.Product.IdSupplier);
                foreach (var group in grouped)
                {
                    var order = new Orders
                    {
                        IdUser = userId,
                        IdAddress = dto.IdAddress,
                        IdStatus = 1, // In lavorazione
                        OrderDate = DateTime.UtcNow,
                        TotalPrice = 0,
                        OrderDetails = new List<OrderDetails>()
                    };

                    foreach (var item in group)
                    {
                        if (item.Product.Quantity < item.Quantity)
                        {
                            response = ResponseBase<bool>.Fail($"Quantità non disponibile per il prodotto {item.Product.Name}", ErrorCode.ValidationError);
                            hasErrors = true;
                            break;
                        }

                        item.Product.Quantity -= item.Quantity;
                        await _productRepository.UpdateAsync(item.Product);

                        order.OrderDetails.Add(new OrderDetails
                        {
                            IdProduct = item.IdProduct,
                            Quantity = item.Quantity,
                            UnitPrice = item.Product.Price,
                        });

                        order.TotalPrice += item.Quantity * item.Product.Price;
                    }

                    if (hasErrors) break;

                    await _repository.AddAsync(order);
                }

                if (!hasErrors)
                {
                    await _cartRepository.ClearCartAsync(userId);
                    response = ResponseBase<bool>.Success(true);
                }
            }
        }

        return response;
    }

    // l'ordine può essere annullato solo se è ancora in lavorazione
    // quando viene annullato, i prodotti vengono restituiti al magazzino
    public async Task<ResponseBase<OrderDTO>> CancelOrderAsync(int orderId)
    {
        var response = new ResponseBase<OrderDTO>();
        var userId = _authorizationService.GetCurrentUserId();
        var (hasPermission, ownOnly) = await _authorizationService.HasPermissionAsync(userId, "CanCancelOrder");

        if (!hasPermission)
        {
            response = ResponseBase<OrderDTO>.Fail("Non sei autorizzato ad annullare ordini.", ErrorCode.Unauthorized);
        }
        else
        {
            var order = await _repository.GetOrderByIdWithDetailsAsync(orderId);
            if (order == null)
            {
                response = ResponseBase<OrderDTO>.Fail("Ordine non trovato.", ErrorCode.NotFound);
            }
            else if (ownOnly && !await _repository.IsOrderOwnedByUserAsync(orderId, userId))
            {
                response = ResponseBase<OrderDTO>.Fail("Non sei il proprietario di questo ordine.", ErrorCode.Unauthorized);
            }
            else if (order.IdStatus == 4)
            {
                response = ResponseBase<OrderDTO>.Fail("L'ordine è già stato annullato.", ErrorCode.ValidationError);
            }
            else if (order.IdStatus != 1)
            {
                response = ResponseBase<OrderDTO>.Fail("L'ordine può essere annullato solo se è ancora in lavorazione.", ErrorCode.ValidationError);
            }
            else
            {
                order.IdStatus = 4; // Annullato
                order.TotalPrice = 0;

                foreach (var item in order.OrderDetails)
                {
                    var product = await _productRepository.GetByIdAsync(item.IdProduct);
                    if (product != null)
                    {
                        product.Quantity += item.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                var updatedOrder = await _repository.UpdateAsync(order);
                response = ResponseBase<OrderDTO>.Success(_mapper.Map<OrderDTO>(updatedOrder));
            }
        }
        return response;
    }

    // aggiorna lo stato dell'ordine solo se il cambiamento è valido
    public async Task<ResponseBase<OrderDTO>> UpdateStatusOrderAsync(int orderId, int newStatusId)
    {
        var response = new ResponseBase<OrderDTO>();
        var userId = _authorizationService.GetCurrentUserId();
        var (hasPermission, ownOnly) = await _authorizationService.HasPermissionAsync(userId, "CanUpdateOrderStatus");

        var order = await _repository.GetOrderByIdWithDetailsAsync(orderId);

        if (order == null)
        {
            response = ResponseBase<OrderDTO>.Fail("Ordine non trovato.", ErrorCode.NotFound);
        }
        else if (!hasPermission)
        {
            response = ResponseBase<OrderDTO>.Fail("Non sei autorizzato a modificare lo stato di questo ordine.", ErrorCode.Unauthorized);
        }
        else if (ownOnly && !await _repository.IsOrderOwnedByUserAsync(orderId, userId))
        {
            response = ResponseBase<OrderDTO>.Fail("Non sei il proprietario di questo ordine.", ErrorCode.Unauthorized);
        }
        else if (order.IdStatus == newStatusId)
        {
            response = ResponseBase<OrderDTO>.Fail("Lo stato dell'ordine è già impostato su questo valore.", ErrorCode.ValidationError);
        }
        else if (order.IdStatus == 3 || order.IdStatus == 4)
        {
            response = ResponseBase<OrderDTO>.Fail("Lo stato di un ordine consegnato o annullato non può essere modificato.", ErrorCode.ValidationError);
        }
        else if ((order.IdStatus == 1 && newStatusId == 2) || (order.IdStatus == 2 && newStatusId == 3))
        {
            order.IdStatus = newStatusId;
            var updatedOrder = await _repository.UpdateAsync(order);
            response = ResponseBase<OrderDTO>.Success(_mapper.Map<OrderDTO>(updatedOrder));
        }
        else
        {
            response = ResponseBase<OrderDTO>.Fail("Transizione di stato non valida.", ErrorCode.ValidationError);
        }
        return response;
    }


    // otteniamo tutti gli ordini con dettagli
    public async Task<ResponseBase<IEnumerable<OrderDTO>>> GetAllOrdersWithDetailsAsync()
    {
        var response = new ResponseBase<IEnumerable<OrderDTO>>();
        var orders = await _repository.GetAllOrdersWithDetails().ToListAsync();

        try
        {
            if (orders == null || !orders.Any())
            {
                response = ResponseBase<IEnumerable<OrderDTO>>.Fail("Nessun ordine trovato.", ErrorCode.NotFound);
            }
            else
            {
                response = ResponseBase<IEnumerable<OrderDTO>>.Success(_mapper.Map<IEnumerable<OrderDTO>>(orders));
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<IEnumerable<OrderDTO>>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }
        return response;
    }

    // otteniamo tutti gli ordini di un utente specifico
    public async Task<ResponseBase<IEnumerable<OrderDTO>>> GetOrdersByUserIdAsync(int userId)
    {
        var response = new ResponseBase<IEnumerable<OrderDTO>>();
        var callerId = _authorizationService.GetCurrentUserId();
        var (hasPermission, ownOnly) = await _authorizationService.HasPermissionAsync(callerId, "CanAccessOrdersByUser");

        if (!hasPermission)
        {
            response = ResponseBase<IEnumerable<OrderDTO>>.Fail("Non sei autorizzato a visualizzare questi ordini.", ErrorCode.Unauthorized);
        }
        else if (ownOnly && userId != callerId)
        {
            response = ResponseBase<IEnumerable<OrderDTO>>.Fail("Non sei autorizzato a visualizzare gli ordini di altri utenti.", ErrorCode.Unauthorized);
        }
        else
        {
            var orders = await _repository.GetOrdersByUserId(userId).ToListAsync();
            response = ResponseBase<IEnumerable<OrderDTO>>.Success(_mapper.Map<IEnumerable<OrderDTO>>(orders ?? new List<Orders>()));

        }
        return response;
    }

    // otteniamo un ordine specifico con dettagli
    public async Task<ResponseBase<OrderDTO>> GetOrderByIdWithDetailsAsync(int orderId)
    {
        var response = new ResponseBase<OrderDTO>();
        var userId = _authorizationService.GetCurrentUserId();
        var (hasPermission, ownOnly) = await _authorizationService.HasPermissionAsync(userId, "CanAccessOrdersByUser");

        if (!hasPermission)
        {
            response = ResponseBase<OrderDTO>.Fail("Non sei autorizzato a visualizzare questo ordine.", ErrorCode.Unauthorized);
        }
        else
        {
            if (ownOnly && !await _repository.IsOrderOwnedByUserAsync(orderId, userId))
            {
                response = ResponseBase<OrderDTO>.Fail("Non sei autorizzato a visualizzare questo ordine.", ErrorCode.Unauthorized);
            }
            else
            {
                var order = await _repository.GetOrderByIdWithDetailsAsync(orderId);
                if (order == null)
                {
                    response = ResponseBase<OrderDTO>.Fail("Ordine non trovato.", ErrorCode.NotFound);
                }
                else
                {
                    response = ResponseBase<OrderDTO>.Success(_mapper.Map<OrderDTO>(order));
                }
            }
        }
        return response;
    }

    public async Task<ResponseBase<IEnumerable<OrderDTO>>> GetOrdersBySupplierAsync()
    {
        var response = new ResponseBase<IEnumerable<OrderDTO>>();
        var userId = _authorizationService.GetCurrentUserId();
        var (hasPermission, _) = await _authorizationService.HasPermissionAsync(userId, "CanAccessOrdersByUser");

        if (!hasPermission)
        {
            response = ResponseBase<IEnumerable<OrderDTO>>.Fail("Non sei autorizzato a visualizzare gli ordini per i tuoi prodotti.", ErrorCode.Unauthorized);
        }
        else
        {
            var orders = await _repository.GetOrdersBySupplier(userId).ToListAsync();
            if (orders == null || !orders.Any())
            {
                response = ResponseBase<IEnumerable<OrderDTO>>.Fail("Nessun ordine trovato per i tuoi prodotti.", ErrorCode.NotFound);
            }
            else
            {
                response = ResponseBase<IEnumerable<OrderDTO>>.Success(_mapper.Map<IEnumerable<OrderDTO>>(orders));
            }
        }
        return response;
    }


}