using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.DTOs;
using MyWarehouse.Common.Response;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.WEB.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    #region GET
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var products = await _orderService.GetAllAsync();
        return Ok(products);
    }

    //Prodotto per ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrdertById(int id)
    {
        var response = await _orderService.GetByIdAsync(id);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }

    // otteniamo tutti gli ordini con dettagli
    [HttpGet("details")]
    public async Task<IActionResult> GetAllOrdersWithDetails()
    {
        var response = await _orderService.GetAllOrdersWithDetailsAsync();
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }

    // otteniamo un ordine con dettagli tramite ID
    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetOrderByIdWithDetails(int id)
    {
        var response = await _orderService.GetOrderByIdWithDetailsAsync(id);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }

    // otteniamo tutti gli ordini di un utente
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetOrdersByUser(int userId)
    {
        var response = await _orderService.GetOrdersByUserIdAsync(userId);
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }

    #endregion

    #region POST
    [Authorize(Roles = "client,supplier")]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderDTO orderDto)
    {
        IActionResult result;
        ResponseBase<OrderDTO> response;

        if (orderDto == null)
        {
            response = ResponseBase<OrderDTO>.Fail("Dati dell'ordine non validi.", ErrorCode.ValidationError);
            result = BadRequest(response);
        }
        else
        {
            response = await _orderService.CreateOrderAsync(orderDto);
            result = response.Result ? CreatedAtAction(nameof(GetOrderByIdWithDetails), new { id = response.Data.Id }, response.Data)
                                     : BadRequest(response.ErrorMessage);
        }

        return result;
    }

    #endregion

    #region PUT

    // aggiorniamo lo stato di un ordine
    [HttpPut("{id}/status/{newStatusId}")]
    public async Task<IActionResult> UpdateOrderStatus(int id, int newStatusId)
    {
        IActionResult result;
        ResponseBase<OrderDTO> response;

        response = await _orderService.UpdateStatusOrderAsync(id, newStatusId);
        result = response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });

        return result;
    }

    // annulliamo un ordine
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        IActionResult result;
        ResponseBase<OrderDTO> response;

        response = await _orderService.CancelOrderAsync(id);
        result = response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });

        return result;
    }

    #endregion
}
