using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Aggregates;

public class Order : AggregateRoot
{
    public int Id { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime PlacedAt { get; private set; }
    public Money Total { get; private set; }

    private readonly List<OrderLine> _lines = new();
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    //Module 2 Clip 7 — This private constructor exists for EF Core materialization.
    // EF bypasses the factory method and writes directly to backing fields when reloading from the DB.
    private Order() { Total = null!; }

    public static Order Place(int customerId, IEnumerable<(int productId, int quantity, Money unitPrice)> lines)
    {
        if (customerId <= 0)
            throw new ArgumentException("A valid customer ID is required.", nameof(customerId));

        ArgumentNullException.ThrowIfNull(lines);

        var linesList = lines.ToList();

        if (linesList.Count == 0)
            throw new ArgumentException("An order must have at least one line.", nameof(lines));

        var order = new Order
        {
            Id = 0,
            Status = OrderStatus.Placed,
            PlacedAt = DateTime.UtcNow
        };

        foreach (var (productId, quantity, unitPrice) in linesList)
        {
            order.AddLine(productId, quantity, unitPrice);
        }

        order.Total = Money.Create(order.Lines.Sum(l => l.LineTotal.Amount), linesList[0].unitPrice.Currency);

        order.RaiseDomainEvent(new OrderPlacedEvent(order.Id, order.PlacedAt));
        return order;
    }

    public void AddLine(int productId, int quantity, Money unitPrice)
    {
        if (productId <= 0)
            throw new ArgumentException("A valid product ID is required.", nameof(productId));
        if (_lines.Any(l => l.ProductId == productId))
            throw new InvalidOperationException($"Product {productId} is already on this order. Update the existing line's quantity instead.");

        _lines.Add(new OrderLine(productId, quantity, unitPrice));
        Total = Money.Create(Lines.Sum(l => l.LineTotal.Amount), unitPrice.Currency);
    }

    public void Process()
    {
        if (Status != OrderStatus.Placed)
            throw new InvalidOperationException($"Cannot process an order in {Status} state. Only Placed orders can be processed.");

        Status = OrderStatus.Processing;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException($"Cannot confirm an order in {Status} state. Only Processing orders can be confirmed.");

        Status = OrderStatus.Confirmed;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot ship an order in {Status} state. Only Confirmed orders can be shipped.");

        Status = OrderStatus.Shipped;
        RaiseDomainEvent(new OrderShippedEvent(Id, DateTime.UtcNow));
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Cannot cancel an order that has already been shipped.");
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled.");

        Status = OrderStatus.Cancelled;
        RaiseDomainEvent(new OrderCancelledEvent(Id, DateTime.UtcNow));
    }
}
