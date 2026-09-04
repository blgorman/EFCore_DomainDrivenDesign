using OrderManagement.Domain.Events;

namespace OrderManagement.Infrastructure.Events;

public interface IDomainEventHandler
{
    Task HandleAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}

public interface IDomainEventHandler<T> : IDomainEventHandler where T : IDomainEvent
{
    Task HandleAsync(T domainEvent, CancellationToken ct = default);
}
