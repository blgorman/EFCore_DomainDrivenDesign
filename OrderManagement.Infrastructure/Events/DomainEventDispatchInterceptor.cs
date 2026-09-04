using Microsoft.EntityFrameworkCore.Diagnostics;
using OrderManagement.Domain.Events;

namespace OrderManagement.Infrastructure.Events;

public class DomainEventDispatchInterceptor : SaveChangesInterceptor
{
    private readonly DomainEventDispatcher _dispatcher;

    public DomainEventDispatchInterceptor(DomainEventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct = default)
    {
        //TODO: Module 5 Clip 3 — Uncomment the implementation below:
        //var context = eventData.Context;
        //if (context is null) return await base.SavedChangesAsync(eventData, result, ct);
        //
        //var holders = context.ChangeTracker
        //    .Entries<IDomainEventHolder>()
        //    .Select(e => e.Entity)
        //    .Where(h => h.DomainEvents.Count > 0)
        //    .ToList();
        //
        //var events = holders
        //    .SelectMany(h => h.DomainEvents)
        //    .ToList();
        //
        //try
        //{
        //    foreach (var evt in events)
        //        await _dispatcher.DispatchAsync(evt, ct);
        //}
        //finally
        //{
        //    foreach (var holder in holders)
        //        holder.ClearEvents();
        //}

        return await base.SavedChangesAsync(eventData, result, ct);
    }
}
