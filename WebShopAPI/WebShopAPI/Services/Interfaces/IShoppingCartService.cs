using WebShopAPI.Models;
using System.Threading.Tasks;

namespace WebShopAPI.Services.Interfaces
{
    public interface IShoppingCartService
    {
        ShoppingCart GetCart();
        void AddToCart(Product item);
        void UpdateCartItem(int productId, int quantity);
        void RemoveFromCart(int productId);
        void ClearCart();
        Task<Order> CheckoutAsync(int customerId);
    }
}
