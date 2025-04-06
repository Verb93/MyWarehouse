using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWarehouse.Common.DTOs;
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
    private readonly IAuthorizationService _authorizationService;
    private readonly IMapper _mapper;

    public OrderService(IOrderRepository repository, IProductRepository productRepository, IAuthorizationService authorizationService, IMapper mapper) : base(repository, mapper)
    {
        _repository = repository;
        _productRepository = productRepository;
        _authorizationService = authorizationService;
        _mapper = mapper;
    }

    // all'inserimento di un nuovo ordine controllo:
    // che i prodotti esistano e abbiano quantità sufficiente
    // che il prezzo unitario venga copiato correttamente
    // che il prezzo totale venga calcolato correttamente
    public async Task<ResponseBase<OrderDTO>> CreateOrderAsync(OrderDTO dto)
    {
        var response = new ResponseBase<OrderDTO>();
        var order = _mapper.Map<Orders>(dto);
        order.OrderDate = DateTime.UtcNow;
        order.OrderDetails ??= new List<OrderDetails>();
        order.IdStatus = 1; // Stato "In lavorazione"

        decimal totalPrice = 0;

        try
        {
            foreach (var item in dto.OrderDetails)
            {
                var product = await _productRepository.GetByIdAsync(item.IdProduct);
                if (product == null || product.Quantity < item.Quantity)
                {
                    response = ResponseBase<OrderDTO>.Fail($"Quantità non disponibile per il prodotto con ID {item.IdProduct}.", ErrorCode.ValidationError);
                }
                else
                {
                    product.Quantity -= item.Quantity;
                    await _productRepository.UpdateAsync(product);

                    var unitPrice = product.Price;
                    totalPrice += item.Quantity * unitPrice;

                    order.OrderDetails.Add(new OrderDetails
                    {
                        IdProduct = item.IdProduct,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice
                    });

                    response = ResponseBase<OrderDTO>.Success(_mapper.Map<OrderDTO>(order));
                }
            }

            order.TotalPrice = totalPrice;
            var createdOrder = await _repository.AddAsync(order);
            response = ResponseBase<OrderDTO>.Success(_mapper.Map<OrderDTO>(createdOrder));
        }
        catch (Exception ex)
        {
            response = ResponseBase<OrderDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    // l'ordine può essere annullato solo se è ancora in lavorazione
    // quando viene annullato, i prodotti vengono restituiti al magazzino
    public async Task<ResponseBase<OrderDTO>> CancelOrderAsync(int orderId, int userId, string role)
    {
        var response = new ResponseBase<OrderDTO>();
        var order = await _repository.GetOrderByIdWithDetailsAsync(orderId);

        try
        {
            if (order == null)
            {
                response = ResponseBase<OrderDTO>.Fail($"Ordine con ID {orderId} non trovato.", ErrorCode.NotFound);
            }
            else if (!await _authorizationService.CanCancelOrderAsync(order, userId, role))
            {
                response = ResponseBase<OrderDTO>.Fail("Non sei autorizzato ad annullare questo ordine.", ErrorCode.Unauthorized);
            }
            else if (order.IdStatus == 4) // Se è già annullato, blocchiamo l'operazione
            {
                response = ResponseBase<OrderDTO>.Fail("L'ordine è già stato annullato.", ErrorCode.ValidationError);
            }
            else if (order.IdStatus != 1)
            {
                response = ResponseBase<OrderDTO>.Fail("L'ordine può essere annullato solo se è ancora in lavorazione.", ErrorCode.ValidationError);
            }
            else
            {
                order.IdStatus = 4; // Stato "Annullato"
                order.TotalPrice = 0; // Se annullato, prezzo totale a 0

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
        catch (Exception ex)
        {
            response = ResponseBase<OrderDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    // aggiorna lo stato dell'ordine solo se il cambiamento è valido
    public async Task<ResponseBase<OrderDTO>> UpdateStatusOrderAsync(int orderId, int newStatusId, int userId, string role)
    {
        var response = new ResponseBase<OrderDTO>();
        var order = await _repository.GetOrderByIdWithDetailsAsync(orderId);

        try
        {
            if (order == null)
            {
                response = ResponseBase<OrderDTO>.Fail($"Ordine con ID {orderId} non trovato.", ErrorCode.NotFound);
            }
            else if (!await _authorizationService.CanUpdateOrderStatusAsync(order, userId, role))
            {
                response = ResponseBase<OrderDTO>.Fail("Non sei autorizzato a modificare lo stato di questo ordine.", ErrorCode.Unauthorized);
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
        }
        catch (Exception ex)
        {
            response = ResponseBase<OrderDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
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
    public async Task<ResponseBase<IEnumerable<OrderDTO>>> GetOrdersByUserIdAsync(
    int userId,
    int callerUserId,
    string role)
    {
        var response = new ResponseBase<IEnumerable<OrderDTO>>();

        try
        {
            var hasAccess = await _authorizationService.CanAccessOrdersByUserAsync(userId, callerUserId, role);

            if (!hasAccess)
            {
                response = ResponseBase<IEnumerable<OrderDTO>>.Fail("Non sei autorizzato a visualizzare questi ordini.", ErrorCode.Unauthorized);
            }
            else
            {
                var orders = await _repository.GetOrdersByUserId(userId).ToListAsync();

                if (orders == null || !orders.Any())
                {
                    response = ResponseBase<IEnumerable<OrderDTO>>.Fail("Nessun ordine trovato per questo utente.", ErrorCode.NotFound);
                }
                else
                {
                    response = ResponseBase<IEnumerable<OrderDTO>>.Success(_mapper.Map<IEnumerable<OrderDTO>>(orders));
                }
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<IEnumerable<OrderDTO>>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }

    // otteniamo un ordine specifico con dettagli
    public async Task<ResponseBase<OrderDTO>> GetOrderByIdWithDetailsAsync(
    int orderId,
    int userId,
    string role)
    {
        var response = new ResponseBase<OrderDTO>();

        try
        {
            var order = await _repository.GetOrderByIdWithDetailsAsync(orderId);

            if (order == null)
            {
                response = ResponseBase<OrderDTO>.Fail("Ordine non trovato.", ErrorCode.NotFound);
            }
            else if (!await _authorizationService.CanAccessOrderWithDetailsAsync(order, userId, role))
            {
                response = ResponseBase<OrderDTO>.Fail("Non sei autorizzato a visualizzare questo ordine.", ErrorCode.Unauthorized);
            }
            else
            {
                response = ResponseBase<OrderDTO>.Success(_mapper.Map<OrderDTO>(order));
            }
        }
        catch (Exception ex)
        {
            response = ResponseBase<OrderDTO>.Fail($"Errore interno: {ex.Message}", ErrorCode.ServiceUnavailable);
        }

        return response;
    }
}