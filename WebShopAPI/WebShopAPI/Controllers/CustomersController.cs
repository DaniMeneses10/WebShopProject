using Microsoft.AspNetCore.Mvc;
using WebShopAPI.Models;
using WebShopAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IRepository<Customer> _customerRepository;

    public CustomersController(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
    {
        var customers = await _customerRepository.GetAllAsync();
        if (customers == null || !customers.Any())
            throw new KeyNotFoundException("No customers found.");

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            throw new KeyNotFoundException($"Customer with ID {id} was not found.");

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create([FromBody] Customer customer)
    {
        if (customer == null || string.IsNullOrWhiteSpace(customer.Name))
            throw new ArgumentException("Invalid customer data. Name is required.");

        var createdCustomer = await _customerRepository.AddAsync(customer);
        return CreatedAtAction(nameof(GetById), new { id = createdCustomer.CustomerID }, createdCustomer);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Customer>> Update(int id, [FromBody] Customer customer)
    {
        if (id != customer.CustomerID)
            throw new ArgumentException("Mismatched customer ID.");

        var existingCustomer = await _customerRepository.GetByIdAsync(id);
        if (existingCustomer == null)
            throw new KeyNotFoundException($"Customer with ID {id} was not found.");

        var updatedCustomer = await _customerRepository.UpdateAsync(customer);
        return Ok(updatedCustomer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _customerRepository.DeleteAsync(id);
        if (!success)
            throw new KeyNotFoundException($"Customer with ID {id} was not found.");

        return Ok(new { Message = "Customer deleted successfully." });
    }
}
