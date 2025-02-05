using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using WebShopAPI.Models;
using WebShopAPI.Services.Interfaces;

namespace WebShopAPI.Services.Implementations
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext?.Session ?? throw new InvalidOperationException("Session is not available.");
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<ProductsOrder> _productsOrderRepository;
        private readonly ApplicationDbContext _context; // ✅ Inject DbContext for Transactions

        private const string CartKey = "ShoppingCart";

        public ShoppingCartService(IHttpContextAccessor httpContextAccessor,
                                   ApplicationDbContext context,
                                   IRepository<Product> productRepository,
                                   IRepository<Order> orderRepository,
                                   IRepository<ProductsOrder> productsOrderRepository)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _productsOrderRepository = productsOrderRepository;
        }

        public ShoppingCart GetCart()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                Console.WriteLine("HttpContext is NULL");
                throw new InvalidOperationException("HttpContext is not available.");
            }

            if (_httpContextAccessor.HttpContext.Session == null)
            {
                Console.WriteLine("Session is NULL - Initializing session...");
                _httpContextAccessor.HttpContext.Session.SetString(CartKey, JsonConvert.SerializeObject(new ShoppingCart()));
            }

            var cartJson = _httpContextAccessor.HttpContext.Session.GetString(CartKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                Console.WriteLine("Cart is NULL or Empty - Creating new ShoppingCart");
                return new ShoppingCart();
            }

            Console.WriteLine($"Cart found before checkout: {cartJson}"); // ✅ Imprimir carrito antes de checkout
            return JsonConvert.DeserializeObject<ShoppingCart>(cartJson);
        }


        public void SaveCart(ShoppingCart cart)
        {
            var json = JsonConvert.SerializeObject(cart);
            Console.WriteLine($"Saving cart to session: {json}");
            _httpContextAccessor.HttpContext.Session.SetString(CartKey, json);
        }


        public async Task AddToCart(ShoppingCartItem item) // ✅ Changed to async Task
        {
            var cart = GetCart();
            var product = await _productRepository.GetByIdAsync(item.ProductID);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {item.ProductID} not found.");

            if (product.Stock < item.Quantity)
                throw new InvalidOperationException($"Not enough stock available for {product.Name}.");

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductID == item.ProductID);
            if (existingItem != null)
                existingItem.Quantity += item.Quantity;
            else
                cart.Items.Add(item);

            SaveCart(cart);
        }

        public void UpdateCartItem(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductID == productId);

            if (item == null)
                throw new KeyNotFoundException($"Product with ID {productId} is not in the cart.");

            item.Quantity = quantity;
            SaveCart(cart);
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.Items.RemoveAll(i => i.ProductID == productId);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            SaveCart(new ShoppingCart());
        }

        public async Task<Order> CheckoutAsync(int customerId)
        {
            string test = ("Session ID at checkout: " + _httpContextAccessor.HttpContext.Session.Id);
            var cart = GetCart();

            if (!cart.Items.Any())
            {
                Console.WriteLine("Cart is empty during checkout!");
                throw new InvalidOperationException("Cannot checkout with an empty cart.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    CustomerID = customerId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cart.Items.Sum(i => i.Quantity * i.Price) // Ensure total is calculated
                };

                order = await _orderRepository.AddAsync(order);
                Console.WriteLine($"New Order Created: OrderID = {order.OrderID}"); // ✅ Debug Log

                await transaction.CommitAsync();
                ClearCart();
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Checkout Error: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }
        }



    }
}
