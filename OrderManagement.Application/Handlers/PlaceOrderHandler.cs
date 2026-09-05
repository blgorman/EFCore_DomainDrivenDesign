using OrderManagement.Application.Commands;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Handlers;

public class PlaceOrderHandler
{
    private readonly IOrderRepository _repo;

    public PlaceOrderHandler(IOrderRepository repo) => _repo = repo;

    public async Task<int> HandleAsync(PlaceOrderCommand cmd, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        if (cmd.Lines is null)
            throw new ArgumentNullException(nameof(cmd.Lines));

        // Domain invariants (non-empty lines, valid customer/product IDs) are enforced by Order.Place().
        var lines = cmd.Lines.Select(l => (l.ProductId, l.Quantity, Money.Create(l.UnitPrice, l.Currency)));
        var order = Order.Place(cmd.CustomerId, lines);
        _repo.Add(order, cmd.CustomerId);
        await _repo.SaveAsync(ct);
        return order.Id;
    }
}
