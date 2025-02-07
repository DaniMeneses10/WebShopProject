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
        //private ISession _session => _httpContextAccessor.HttpContext?.Session ?? throw new InvalidOperationException("Session is not available.");
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
            if (_httpContextAccessor.HttpContext?.Session == null)
            {
                Console.WriteLine("❌ Session is NULL.");
                throw new InvalidOperationException("Session is not available.");
            }

            var cartJson = _httpContextAccessor.HttpContext.Session.GetString(CartKey);
            Console.WriteLine($"🔍 Cart JSON retrieved: {cartJson}");

            if (string.IsNullOrEmpty(cartJson))
            {
                Console.WriteLine("⚠️ Cart is NULL or empty. Initializing new cart.");
                var newCart = new ShoppingCart();
                SaveCart(newCart);
                return newCart;
            }

            return JsonConvert.DeserializeObject<ShoppingCart>(cartJson);
        }


        public void SaveCart(ShoppingCart cart)
        {
            var json = JsonConvert.SerializeObject(cart);
            Console.WriteLine($"Saving cart to session: {json}");
            _httpContextAccessor.HttpContext.Session.SetString(CartKey, json);
        }


        public void AddToCart(Product product)
        {
            var cart = GetCart(); // Recuperar carrito existente
            Console.WriteLine($"🔍 Cart before adding: {JsonConvert.SerializeObject(cart)}");

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductID == product.ProductID);
            if (existingItem != null)
            {
                existingItem.Quantity++;
                Console.WriteLine($"🛒 Increased quantity for {product.Name}: {existingItem.Quantity}");
            }
            else
            {
                cart.Items.Add(new ShoppingCartItem { ProductID = product.ProductID, Name = product.Name, Quantity = 1, Price = product.Price });
                Console.WriteLine($"✅ Added new item: {product.Name}");
            }

            SaveCart(cart); // Guardar el carrito actualizado
            Console.WriteLine($"💾 Cart after adding: {JsonConvert.SerializeObject(cart)}");
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
            {
                Console.WriteLine("Cart is empty during checkout!");
                throw new InvalidOperationException("Cannot checkout with an empty cart.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create the main order
                var order = new Order
                {
                    CustomerID = customerId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cart.Items.Sum(i => i.Quantity * i.Price) // Calculate total amount
                };

                order = await _orderRepository.AddAsync(order);
                Console.WriteLine($"New Order Created: OrderID = {order.OrderID}");

                // Process each item in the cart
                foreach (var item in cart.Items)
                {
                    // Validate product stock
                    var product = await _productRepository.GetByIdAsync(item.ProductID);

                    if (product == null)
                        throw new KeyNotFoundException($"Product with ID {item.ProductID} not found.");

                    if (product.Stock < item.Quantity)
                        throw new InvalidOperationException($"Not enough stock for product {product.Name}. Available: {product.Stock}, Requested: {item.Quantity}");

                    // Deduct stock
                    product.Stock -= item.Quantity;
                    await _productRepository.UpdateAsync(product);

                    // Create an entry in ProductsOrder
                    var productsOrder = new ProductsOrder
                    {
                        OrderID = order.OrderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        LineTotal = item.Quantity * item.Price // Calculate LineTotal
                    };

                    await _productsOrderRepository.AddAsync(productsOrder);

                    Console.WriteLine($"Added to order: ProductID = {item.ProductID}, Quantity = {item.Quantity}, LineTotal = {productsOrder.LineTotal}");
                }

                // Commit the transaction
                await transaction.CommitAsync();

                // Clear the cart after checkout
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
