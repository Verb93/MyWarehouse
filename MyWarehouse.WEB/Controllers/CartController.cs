using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWarehouse.Common.Security;
using MyWarehouse.Interfaces.ServiceInterfaces;

namespace MyWarehouse.Common.Controllers;

[Authorize(Policy = Policies.Client)]
[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    #region GET
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var response = await _cartService.GetCartAsync();
        return response.Result ? Ok(response.Data) : BadRequest(new { message = response.ErrorMessage });
    }

    [HttpGet("checkout")]
    public async Task<IActionResult> Checkout([FromQuery] int idAddress)
    {
        IActionResult result;
        var serviceResult = await _cartService.CheckoutAsync(idAddress);

        if (!serviceResult.Result)
        {
            result = StatusCode((int)serviceResult.ErrorCode, new { message = serviceResult.ErrorMessage });
        }
        else
        {
            result = Ok(serviceResult.Data);
        }

        return result;
    }

    #endregion

    #region POST
    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (request == null || request.IdProduct <= 0 || request.Quantity <= 0)
        {
            return BadRequest(new { message = "Dati non validi" });
        }

        var response = await _cartService.AddToCartAsync(request);
        return response.Result ? Ok(new { message = "Prodotto aggiunto al carrello" }) : BadRequest(new { message = response.ErrorMessage });
    }
    #endregion

    #region DELETE
    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var response = await _cartService.RemoveFromCartAsync(productId);
        return response.Result ? Ok(new { message = "Prodotto rimosso dal carrello" }) : BadRequest(new { message = response.ErrorMessage });
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var response = await _cartService.ClearCartAsync();
        return response.Result ? Ok(new { message = "Carrello svuotato" }) : BadRequest(new { message = response.ErrorMessage });
    }
    #endregion
}