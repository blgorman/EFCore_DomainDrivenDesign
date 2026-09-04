using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Commands;
using OrderManagement.Application.Handlers;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly PlaceOrderHandler _placeOrderHandler;
    private readonly IOrderRepository _repo;

    public OrdersController(PlaceOrderHandler placeOrderHandler, IOrderRepository repo)
    {
        _placeOrderHandler = placeOrderHandler;
        _repo = repo;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderCommand command, CancellationToken ct)
    {
        try
        {
            var id = await _placeOrderHandler.HandleAsync(command, ct);
            return CreatedAtAction(nameof(GetOrder), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    //Note: Reminder to discuss how this leaks the full aggregate structure and 
    // how to use a DTO instead for the API response, 
    // to avoid tight coupling between API and domain model.
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(id, ct);
        if (order is null) return NotFound();
        return Ok(order);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int id, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(id, ct);
        if (order is null) return NotFound();
        try
        {
            order.Cancel();
            await _repo.SaveAsync(ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:int}/ship")]
    public async Task<IActionResult> ShipOrder(int id, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(id, ct);
        if (order is null) return NotFound();
        try
        {
            order.Ship();
            await _repo.SaveAsync(ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
