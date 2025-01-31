using System.Collections.Generic;
using System.Linq;

namespace WebShopAPI.Models
{
    public class ShoppingCart
    {
        public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();

        public decimal Total => Items.Sum(item => item.Quantity * item.Price);
    }

    public class ShoppingCartItem
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
