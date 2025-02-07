using Microsoft.EntityFrameworkCore;
using WebShopAPI.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<ProductsOrder> ProductsOrder { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Define composite primary key for ProductsOrder
        modelBuilder.Entity<ProductsOrder>()
            .HasKey(po => new { po.OrderID, po.ProductID }); // Composite key
    }
}
