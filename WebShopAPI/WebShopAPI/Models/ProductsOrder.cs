using System.ComponentModel.DataAnnotations.Schema;

namespace WebShopAPI.Models
{
    public class ProductsOrder
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }
    }
}
