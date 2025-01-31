using Microsoft.AspNetCore.Mvc;
using WebShopAPI.Models;
using WebShopAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IRepository<Order> _orderRepository;

    public OrdersController(IRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll()
    {
        var orders = await _orderRepository.GetAllAsync();
        if (orders == null || !orders.Any())
            throw new KeyNotFoundException("No orders found.");

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetById(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID {id} was not found.");

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] Order order)
    {
        if (order == null || order.CustomerID <= 0)
            throw new ArgumentException("Invalid order data. Ensure all required fields are filled correctly.");

        var createdOrder = await _orderRepository.AddAsync(order);
        return CreatedAtAction(nameof(GetById), new { id = createdOrder.OrderID }, createdOrder);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Order>> Update(int id, [FromBody] Order order)
    {
        if (id != order.OrderID)
            throw new ArgumentException("Mismatched order ID.");

        var existingOrder = await _orderRepository.GetByIdAsync(id);
        if (existingOrder == null)
            throw new KeyNotFoundException($"Order with ID {id} was not found.");

        var updatedOrder = await _orderRepository.UpdateAsync(order);
        return Ok(updatedOrder);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _orderRepository.DeleteAsync(id);
        if (!success)
            throw new KeyNotFoundException($"Order with ID {id} was not found.");

        return Ok(new { Message = "Order deleted successfully." });
    }
}
