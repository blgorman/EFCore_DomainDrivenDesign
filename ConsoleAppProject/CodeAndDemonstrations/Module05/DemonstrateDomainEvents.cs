using ConsoleHelpers;
using OrderManagement.Domain.Events;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateDomainEvents
{
    public static async Task WalkInterfacesAsync()
    {
        // ── Step 1: Core concepts + inspect IDomainEvent via reflection ───────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Clip 1 — What Are Domain Events and Where Do They Live?"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Domain Events — Core Concepts",
            new[]
            {
                "A Domain Event is an immutable fact about something that happened in the domain.",
                "It is fully owned by the domain layer — zero infrastructure dependencies.",
                "Defined in OrderManagement.Domain.Events namespace.",
                "",
                "Domain Events decouple business logic from side effects:",
                "  Business logic:  Order.Place() sets status, adds lines, raises OrderPlacedEvent",
                "  Side effect:     email notification, projection update, logging",
                "  Key point:       the domain method never calls the side-effect code directly"
            }
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 2: Inspect IDomainEvent interface via reflection ─────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 2 — Inspect IDomainEvent via Reflection"),
            ConsoleColor.DarkBlue);

        var iface = typeof(IDomainEvent);
        var declaredMembers = iface.GetMembers().Length;
        var baseInterfaces = iface.GetInterfaces().Length;

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "IDomainEvent — Reflection Results",
            new[]
            {
                $"Full name:           {iface.FullName}",
                $"Namespace:           {iface.Namespace}",
                $"Declared members:    {declaredMembers}  — intentionally zero (marker interface)",
                $"Base interfaces:     {baseInterfaces}  — extends nothing",
                "",
                "A marker interface signals category membership, not a method contract.",
                "IDomainEvent means: 'this type represents a domain fact — nothing more.",
                "Infrastructure discovers events via IDomainEventHolder, not IDomainEvent directly."
            }
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 3: Inspect OrderPlacedEvent ─────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 3 — OrderPlacedEvent: Immutable Record in Domain.Events"),
            ConsoleColor.DarkBlue);

        var evtType = typeof(OrderPlacedEvent);
        var isRecord = evtType.GetMethod("<Clone>$") is not null;
        var isAssignable = typeof(IDomainEvent).IsAssignableFrom(evtType);
        var liveEvt = new OrderPlacedEvent(Random.Shared.Next(10000, 99999), DateTime.UtcNow);
        var props = evtType.GetProperties()
            .Select(p => $"    {p.Name,-12} ({p.PropertyType.Name})")
            .ToArray();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderPlacedEvent — Runtime Inspection",
            new[]
            {
                $"Type:                    {evtType.Name}",
                $"Is record (immutable):   {isRecord}",
                $"Implements IDomainEvent: {isAssignable}",
                $"Namespace:               {evtType.Namespace}",
                "---",
                "Properties (the facts captured at event-raise time):"
            }
            .Concat(props)
            .Concat(new[]
            {
                "---",
                "Live instance values:",
                $"    OrderId:  {liveEvt.OrderId}   (<= randomly generated, not a saved order)",
                $"    PlacedAt: {liveEvt.PlacedAt:u}"
            })
            .ToArray()
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 4: Discover all IDomainEvent implementations at runtime ──────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 4 — Discover All Domain Event Types at Runtime"),
            ConsoleColor.DarkBlue);

        var eventTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IDomainEvent).IsAssignableFrom(t))
            .Select(t => $"    {t.Name,-30} [{t.Namespace}]")
            .OrderBy(n => n)
            .ToArray();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "IDomainEvent Implementations — Discovered via AppDomain",
            new[]
            {
                $"Concrete types implementing IDomainEvent: {eventTypes.Length}",
                "---"
            }
            .Concat(eventTypes)
            .Concat(new[]
            {
                "---",
                "All events live in OrderManagement.Domain.Events.",
                "No infrastructure types implement IDomainEvent — domain stays independent."
            })
            .ToArray()
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 5: Domain Event Lifecycle overview ────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 5 — Domain Event Lifecycle: 5 Stages"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Domain Event Lifecycle — End to End",
            new[]
            {
                "Stage 1: Event CREATED inside a domain method",
                "         Order.Place() calls RaiseDomainEvent(new OrderPlacedEvent(...))",
                "",
                "Stage 2: Event QUEUED on the aggregate root until SaveChanges",
                "         Stored in AggregateRoot._domainEvents — not yet dispatched",
                "",
                "Stage 3: INTERCEPTOR fires after SaveChanges commits",
                "         DomainEventDispatchInterceptor.SavedChangesAsync() collects events",
                "",
                "Stage 4: HANDLER runs — side effects happen",
                "         OrderPlacedEventHandler logs the order ID via ILogger",
                "",
                "Stage 5: Events CLEARED from the aggregate",
                "         holder.ClearEvents() — prevents double-dispatch on future saves"
            }
        ));

        await Task.CompletedTask;
    }
}
