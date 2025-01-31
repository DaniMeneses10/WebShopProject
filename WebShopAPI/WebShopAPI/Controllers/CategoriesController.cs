using Microsoft.AspNetCore.Mvc;
using WebShopAPI.Models;
using WebShopAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IRepository<Category> _categoryRepository;

    public CategoriesController(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll()
    {
        var categories = await _categoryRepository.GetAllAsync();
        if (categories == null || !categories.Any())
            throw new KeyNotFoundException("No categories found.");

        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetById(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new KeyNotFoundException($"Category with ID {id} was not found.");

        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> Create([FromBody] Category category)
    {
        if (category == null || string.IsNullOrWhiteSpace(category.Name))
            throw new ArgumentException("Invalid category data. Name is required.");

        var createdCategory = await _categoryRepository.AddAsync(category);
        return CreatedAtAction(nameof(GetById), new { id = createdCategory.CategoryID }, createdCategory);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Category>> Update(int id, [FromBody] Category category)
    {
        if (id != category.CategoryID)
            throw new ArgumentException("Mismatched category ID.");

        var existingCategory = await _categoryRepository.GetByIdAsync(id);
        if (existingCategory == null)
            throw new KeyNotFoundException($"Category with ID {id} was not found.");

        var updatedCategory = await _categoryRepository.UpdateAsync(category);
        return Ok(updatedCategory);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _categoryRepository.DeleteAsync(id);
        if (!success)
            throw new KeyNotFoundException($"Category with ID {id} was not found.");

        return Ok(new { Message = "Category deleted successfully." });
    }
}
