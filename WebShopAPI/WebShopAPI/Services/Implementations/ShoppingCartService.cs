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
            var cartJson = _session.GetString(CartKey);
            return cartJson == null ? new ShoppingCart() : JsonConvert.DeserializeObject<ShoppingCart>(cartJson);
        }

        public void SaveCart(ShoppingCart cart)
        {
            _session.SetString(CartKey, JsonConvert.SerializeObject(cart));
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
            var cart = GetCart();
            if (!cart.Items.Any())
                throw new InvalidOperationException("Cannot checkout with an empty cart.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    CustomerID = customerId,
                    OrderDate = DateTime.UtcNow
                };

                order = await _orderRepository.AddAsync(order);

                foreach (var item in cart.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductID);
                    if (product == null)
                        throw new KeyNotFoundException($"Product with ID {item.ProductID} no longer exists.");

                    if (product.Stock < item.Quantity)
                        throw new InvalidOperationException($"Not enough stock available for {product.Name}.");

                    product.Stock -= item.Quantity;
                    await _productRepository.UpdateAsync(product);

                    var productsOrder = new ProductsOrder
                    {
                        OrderID = order.OrderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        LineTotal = item.Quantity * item.Price
                    };

                    await _productsOrderRepository.AddAsync(productsOrder);
                }

                await transaction.CommitAsync();
                ClearCart();
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
