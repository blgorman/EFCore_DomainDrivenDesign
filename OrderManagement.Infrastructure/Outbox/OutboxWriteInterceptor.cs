using System.Text.Json;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OrderManagement.Domain.Events;

namespace OrderManagement.Infrastructure.Outbox;

public class OutboxWriteInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        var context = eventData.Context;
        if (context is null) return base.SavingChangesAsync(eventData, result, ct);

        var holders = context.ChangeTracker
            .Entries<IDomainEventHolder>()
            .Select(e => e.Entity)
            .Where(h => h.DomainEvents.Count > 0)
            .ToList();

        // Rows are added before the save runs, so they commit in the same
        // transaction as the aggregate state that raised the events.
        foreach (var evt in holders.SelectMany(h => h.DomainEvents))
        {
            context.Set<OutboxMessage>().Add(new OutboxMessage
            {
                Type          = evt.GetType().Name,
                Payload       = JsonSerializer.Serialize(evt, evt.GetType()),
                OccurredAtUtc = DateTime.UtcNow
            });
        }

        foreach (var holder in holders)
            holder.ClearEvents();

        return base.SavingChangesAsync(eventData, result, ct);
    }
}
