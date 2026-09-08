using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Events;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Infrastructure.Events;

// Design note: this handler injects ShippingContext directly rather than through an
// IShipmentRepository abstraction. That is intentional for this demo: ShipmentReadModel
// is a projection / read model, not a domain aggregate. Read models are commonly accessed
// via DbContext directly rather than through the repository pattern, which is reserved for
// aggregate roots. The Module 5 demo calls this out explicitly.
public class OrderShippedEventHandler : IDomainEventHandler<OrderShippedEvent>
{
    private readonly ShippingContext _shippingContext;
    private readonly ILogger<OrderShippedEventHandler> _logger;

    public OrderShippedEventHandler(ShippingContext shippingContext, ILogger<OrderShippedEventHandler> logger)
    {
        _shippingContext = shippingContext;
        _logger = logger;
    }

    Task IDomainEventHandler.HandleAsync(IDomainEvent domainEvent, CancellationToken ct)
        => HandleAsync((OrderShippedEvent)domainEvent, ct);

    public async Task HandleAsync(OrderShippedEvent domainEvent, CancellationToken ct = default)
    {
        var shipment = await _shippingContext.Shipments
            .FirstOrDefaultAsync(s => s.Id == domainEvent.OrderId, ct);

        if (shipment is not null)
        {
            _logger.LogInformation(
                "ShippingContext projection — Order {OrderId}: Status={Status}, PlacedAt={PlacedAt}",
                shipment.Id, shipment.Status, shipment.PlacedAt);
        }
        else
        {
            _logger.LogWarning(
                "ShippingContext could not find a projection for Order {OrderId}",
                domainEvent.OrderId);
        }
    }
}
