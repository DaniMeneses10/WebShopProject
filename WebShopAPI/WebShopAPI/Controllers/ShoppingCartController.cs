using Microsoft.AspNetCore.Mvc;
using WebShopAPI.Models;
using WebShopAPI.Services.Interfaces;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _cartService;

    public ShoppingCartController(IShoppingCartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public ActionResult<ShoppingCart> GetCart()
    {
        var cart = _cartService.GetCart();

        if (cart == null || cart.Items.Count == 0)
        {
            return Ok(new { message = "Cart is empty", items = new List<ShoppingCartItem>() });
        }

        return Ok(cart);
    }


    [HttpPost("add")]
    public IActionResult AddToCart([FromBody] ShoppingCartItem item)
    {
        _cartService.AddToCart(item);
        return Ok(new { Message = "Product added to cart successfully." });
    }

    [HttpPut("update/{productId}")]
    public IActionResult UpdateCartItem(int productId, [FromBody] int quantity)
    {
        _cartService.UpdateCartItem(productId, quantity);
        return Ok(new { Message = "Cart item updated successfully." });
    }

    [HttpDelete("remove/{productId}")]
    public IActionResult RemoveFromCart(int productId)
    {
        _cartService.RemoveFromCart(productId);
        return Ok(new { Message = "Product removed from cart." });
    }

    [HttpDelete("clear")]
    public IActionResult ClearCart()
    {
        _cartService.ClearCart();
        return Ok(new { Message = "Shopping cart cleared." });
    }

    [HttpPost("checkout/{customerId}")]
    public async Task<IActionResult> Checkout(int customerId)
    {
        var order = await _cartService.CheckoutAsync(customerId);
        return Ok(new { Message = "Order placed successfully!", OrderID = order.OrderID });
    }
}
