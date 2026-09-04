using ConsoleHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;
using System.Reflection;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateDomainEventCollection
{
    public static async Task ShowEventCollectionAsync(IServiceProvider serviceProvider)
    {
        // Seed customers so CustomerAId is a real database id — the order itself is never saved.
        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.EnsureCustomersAsync(seedCtx);
        }

        // ── Step 1: IDomainEventHolder — interface and AggregateRoot wiring ──
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Clip 2 — Collecting and Managing Events on the Aggregate Root"),
            ConsoleColor.DarkBlue);

        var holderType = typeof(IDomainEventHolder);
        // Skip the compiler-generated get_DomainEvents accessor so the two declared members show.
        var holderMembers = holderType.GetMembers()
            .Where(m => m.DeclaringType == holderType)
            .Where(m => m is not MethodInfo method || !method.IsSpecialName)
            .OrderBy(m => m.MemberType == MemberTypes.Property ? 0 : 1)
            .Select(m => $"    {m.MemberType,-10}  {m.Name}")
            .ToArray();

        var aggregateRootType = typeof(AggregateRoot);
        var implementsHolder = holderType.IsAssignableFrom(aggregateRootType);
        var privateField = aggregateRootType
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(f => f.Name == "_domainEvents");

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "IDomainEventHolder — Interface Members and AggregateRoot Wiring",
            new[]
            {
                $"Interface: {holderType.FullName}",
                $"Members declared on IDomainEventHolder ({holderMembers.Length}):",
                "---"
            }
            .Concat(holderMembers)
            .Concat(new[]
            {
                "---",
                $"AggregateRoot implements IDomainEventHolder: {implementsHolder}",
                $"Private field '_domainEvents' found in AggregateRoot: {privateField is not null}",
                $"Field type: {(privateField is null ? "not found" : FriendlyTypeName(privateField.FieldType))}",
                "",
                "External code gets IReadOnlyList<IDomainEvent> — read-only.",
                "Only the aggregate root and the interceptor may add or clear events."
            })
            .ToArray()
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 2: Call Order.Place — event queued on the aggregate ─────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 2 — Call Order.Place(): Domain Event Queued on Aggregate"),
            ConsoleColor.DarkBlue);

        var order = Order.Place(
            SeedDataHelper.CustomerAId,
            new[] { (SeedDataHelper.Product1Id, 2, Money.Create(29.99m, "USD")) }
        );

        var eventNames = order.DomainEvents
            .Select(e => $"    {e.GetType().Name}")
            .ToArray();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Events Queued on Aggregate (before SaveChanges)",
            new[]
            {
                "Order.Place() returned — aggregate created in memory",
                $"Order Id:      {order.Id}",
                $"Events queued: {order.DomainEvents.Count}",
                "---",
                "Event types on DomainEvents (IReadOnlyList<IDomainEvent>):"
            }
            .Concat(eventNames)
            .Concat(new[]
            {
                "---",
                "Events sit on the aggregate until SaveChangesAsync triggers the interceptor.",
                "No handler has been called yet — side effects are deferred."
            })
            .ToArray()
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 3: Simulate post-dispatch — call ClearEvents ─────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 3 — Simulate Post-Dispatch: Call order.ClearEvents()"),
            ConsoleColor.DarkBlue);

        var countBefore = order.DomainEvents.Count;
        order.ClearEvents();
        var countAfter = order.DomainEvents.Count;

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Events After ClearEvents()",
            new[]
            {
                $"Events before ClearEvents(): {countBefore}",
                $"Events after  ClearEvents(): {countAfter}",
                "",
                "In production: DomainEventDispatchInterceptor calls ClearEvents() after dispatch.",
                "This prevents double-dispatch when SaveChanges is called again on the same context.",
                "",
                "Events are raised only inside domain methods — never from application code.",
                "Application code only calls repository.SaveAsync() — the rest is automatic."
            }
        ));

        await Task.CompletedTask;
    }

    private static string FriendlyTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var name = type.Name[..type.Name.IndexOf('`')];
        var args = string.Join(", ", type.GetGenericArguments().Select(FriendlyTypeName));
        return $"{name}<{args}>";
    }
}
