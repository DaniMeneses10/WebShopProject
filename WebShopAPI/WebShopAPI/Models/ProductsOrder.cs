namespace WebShopAPI.Models
{
    public class ProductsOrder
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
