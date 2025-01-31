using WebShopAPI.Models;
using System.Threading.Tasks;

namespace WebShopAPI.Services.Interfaces
{
    public interface IShoppingCartService
    {
        ShoppingCart GetCart();
        Task AddToCart(ShoppingCartItem item); // ✅ Changed to async Task
        void UpdateCartItem(int productId, int quantity);
        void RemoveFromCart(int productId);
        void ClearCart();
        Task<Order> CheckoutAsync(int customerId);
    }
}
