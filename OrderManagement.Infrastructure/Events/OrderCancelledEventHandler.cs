using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Events;

namespace OrderManagement.Infrastructure.Events;

public class OrderCancelledEventHandler : IDomainEventHandler<OrderCancelledEvent>
{
    private readonly ILogger<OrderCancelledEventHandler> _logger;

    public OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(OrderCancelledEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Order cancelled: {OrderId} at {CancelledAt}", domainEvent.OrderId, domainEvent.CancelledAt);
        await Task.CompletedTask;
    }

    Task IDomainEventHandler.HandleAsync(IDomainEvent domainEvent, CancellationToken ct)
        => HandleAsync((OrderCancelledEvent)domainEvent, ct);
}
