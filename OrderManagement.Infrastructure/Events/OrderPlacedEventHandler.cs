using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Events;

namespace OrderManagement.Infrastructure.Events;

public class OrderPlacedEventHandler : IDomainEventHandler<OrderPlacedEvent>
{
    private readonly ILogger<OrderPlacedEventHandler> _logger;

    public OrderPlacedEventHandler(ILogger<OrderPlacedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Order placed: {OrderId}", domainEvent.OrderId);
        await Task.CompletedTask;
    }

    Task IDomainEventHandler.HandleAsync(IDomainEvent domainEvent, CancellationToken ct)
        => HandleAsync((OrderPlacedEvent)domainEvent, ct);
}
