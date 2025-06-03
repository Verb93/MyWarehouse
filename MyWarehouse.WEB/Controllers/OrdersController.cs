using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.Security;
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
    // Solo l'admin può vedere tutti gli ordini con dettagli
    [Authorize(Policy = Policies.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var response = await _orderService.GetAllOrdersWithDetailsAsync();
        return response.Result ? Ok(response.Data) : NotFound(new { message = response.ErrorMessage });
    }

    // Otteniamo un ordine con dettagli tramite ID (Admin, Supplier, Client)
    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetOrderByIdWithDetails(int id)
    {
        var response = await _orderService.GetOrderByIdWithDetailsAsync(id);
        return response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });
    }

    // Otteniamo tutti gli ordini di un utente (controllo ownership nella service)
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetOrdersByUser(int userId)
    {
        var response = await _orderService.GetOrdersByUserIdAsync(userId);
        return response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });
    }

    // Otteniamo gli ordini relativi ai prodotti del supplier (ownership gestita nella service)
    [Authorize(Policy = Policies.Supplier)]
    [HttpGet("supplier-orders")]
    public async Task<IActionResult> GetSupplierOrders()
    {
        var response = await _orderService.GetOrdersBySupplierAsync();
        return response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });
    }
    #endregion

    #region POST
    // Solo il client può creare un ordine
    [Authorize(Policy = Policies.Client)]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CheckoutRequest request)
    {
        IActionResult result;
        var response = await _orderService.CreateOrderAsync(request);

        if (!response.Result)
        {
            result = StatusCode((int)response.ErrorCode, response.ErrorMessage);
        }
        else
        {
            result = Ok(true);
        }

        return result;
    }
    #endregion

    #region PUT
    // Aggiorniamo lo stato di un ordine (Admin o Supplier)
    [Authorize(Policy = Policies.AdminOrSupplier)]
    [HttpPut("{id}/status/{newStatusId}")]
    public async Task<IActionResult> UpdateOrderStatus(int id, int newStatusId)
    {
        var response = await _orderService.UpdateStatusOrderAsync(id, newStatusId);
        return response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });
    }

    // Annulliamo un ordine
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var response = await _orderService.CancelOrderAsync(id);
        return response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });
    }
    #endregion
}
