using Microsoft.AspNetCore.Mvc;
using WebShopAPI.Models;
using WebShopAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IRepository<Product> _productRepository;

    public ProductsController(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _productRepository.GetAllAsync();
        if (products == null || !products.Any())
            return NotFound("No products found.");

        return Ok(products);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} was not found.");

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] Product product)
    {
        if (product == null || string.IsNullOrWhiteSpace(product.Name) || product.Price <= 0)
            throw new ArgumentException("Invalid product data. Ensure all required fields are filled correctly.");

        var createdProduct = await _productRepository.AddAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = createdProduct.ProductID }, createdProduct);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> Update(int id, [FromBody] Product product)
    {
        if (id != product.ProductID)
            throw new ArgumentException("Mismatched product ID.");

        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct == null)
            throw new KeyNotFoundException($"Product with ID {id} was not found.");

        var updatedProduct = await _productRepository.UpdateAsync(product);
        return Ok(updatedProduct);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _productRepository.DeleteAsync(id);
        if (!success)
            throw new KeyNotFoundException($"Product with ID {id} was not found.");

        return Ok(new { Message = "Product deleted successfully." });
    }
}
