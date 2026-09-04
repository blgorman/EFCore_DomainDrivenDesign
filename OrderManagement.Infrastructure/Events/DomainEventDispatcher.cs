using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Events;

namespace OrderManagement.Infrastructure.Events;

public class DomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        //TODO: Module 5 Clip 4 — Uncomment the implementation below:
        //var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        //var handlers = _serviceProvider.GetServices(handlerType);
        //foreach (var handler in handlers)
        //{
        //    if (handler is IDomainEventHandler h)
        //        await h.HandleAsync(domainEvent, ct);
        //}

        await Task.CompletedTask;
    }
}
