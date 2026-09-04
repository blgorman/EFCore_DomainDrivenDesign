using ConsoleHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Events;
using System.Reflection;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateSaveChangesInterceptor
{
    public static async Task ShowInterceptorAsync(IServiceProvider serviceProvider)
    {
        // Seed reference data so the context is in a valid state
        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(seedCtx);
        }

        // ── Step 1: Reflect on DomainEventDispatchInterceptor ─────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Clip 3 — DomainEventDispatchInterceptor: Setup and Registration"),
            ConsoleColor.DarkBlue);

        var interceptorType = typeof(DomainEventDispatchInterceptor);
        var baseType = interceptorType.BaseType;
        var declaredOverrides = interceptorType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(m => $"    override  {m.Name}")
            .ToArray();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "DomainEventDispatchInterceptor — Reflection Results",
            new[]
            {
                $"Type:        {interceptorType.Name}",
                $"Base class:  {baseType?.Name}  (Microsoft.EntityFrameworkCore.Diagnostics)",
                "",
                "Overridden methods declared directly on the interceptor:",
                "---"
            }
            .Concat(declaredOverrides)
            .Concat(new[]
            {
                "---",
                "SavedChangesAsync overridden  — dispatch fires POST-commit.",
                "SavingChangesAsync NOT overridden — pre-save dispatch is not used.",
                "Registered in Program.cs via .AddInterceptors(DomainEventDispatchInterceptor)."
            })
            .ToArray()
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 2: Create the order and inspect queued events ─────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 2 — Order.Place() Queues OrderPlacedEvent on the Aggregate"),
            ConsoleColor.DarkBlue);

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        var order = Order.Place(
            SeedDataHelper.CustomerAId,
            new[] { (SeedDataHelper.Product1Id, 1, Money.Create(29.99m, "USD")) }
        );

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Before SaveChanges — Event Sitting on the Aggregate",
            new[]
            {
                $"Order Id:                 {order.Id}",
                $"Events queued on aggregate: {order.DomainEvents.Count}",
                $"Event type:               {order.DomainEvents[0].GetType().Name}",
                "",
                "No database write yet. No handler has been called yet.",
                "The event lives in AggregateRoot._domainEvents — waiting for SaveChanges."
            }
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 3: Call SaveChangesAsync — interceptor fires automatically ────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 3 — Calling SaveChangesAsync: Interceptor Fires Automatically"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "What the Interceptor Will Do Inside SaveChangesAsync",
            new[]
            {
                "1. DB write commits (INSERT issued for the order row)",
                "2. SavedChangesAsync override fires on DomainEventDispatchInterceptor",
                "3. Interceptor scans ChangeTracker for IDomainEventHolder entities",
                "4. Calls DomainEventDispatcher.DispatchAsync(OrderPlacedEvent)",
                "5. OrderPlacedEventHandler.HandleAsync() runs — logs 'Order placed: {OrderId}'",
                "6. Interceptor calls holder.ClearEvents() — aggregate is clean"
            }
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // The two-argument overload sets the CustomerId shadow FK, which Order.Place() never sets.
        repo.Add(order, SeedDataHelper.CustomerAId);
        await repo.SaveAsync();

        if (order.Id == 0)
        {
            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "OrderRepository Is Not Implemented Yet",
                new[]
                {
                    "repo.SaveAsync() issued no INSERT, so there is nothing for this demo to verify.",
                    "Implement the Module 4 code to see this demo work correctly.",
                    "OrderRepository.Add and OrderRepository.SaveAsync are stubs until Module 4 Clip 4."
                }
            ));
            return;
        }

        // ── Step 4: Verify results — events cleared, DB row committed ─────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 4 — After SaveChanges: Events Cleared, DB Row Committed"),
            ConsoleColor.DarkBlue);

        var savedOrder = await context.Orders.FindAsync(order.Id);

        //TODO: Module 5 Clip 3 — Delete this Not Yet Implemented box:
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This demo will work after completing Module 5 Clip 3.",
                "Open DemonstrateSaveChangesInterceptor.cs and uncomment the //TODO: Module 5 Clip 3 block.",
                "Prerequisite: DomainEventDispatchInterceptor.SavedChangesAsync must be implemented."
            }
        ));

        //TODO: Module 5 Clip 3 — Uncomment the verified-results box below:
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //"Interceptor Has Fired — Verified Results",
        //new[]
        //{
        //    $"Events remaining on aggregate:  {order.DomainEvents.Count}  — ClearEvents() was called",
        //    $"DB row found via FindAsync:      {savedOrder is not null}",
        //    $"Saved Order Id:                 {savedOrder?.Id}",
        //    "",
        //    "The interceptor ran SavedChangesAsync — AFTER the commit.",
        //    "DB write is final before any handler runs.",
        //    "If the handler had thrown, the DB row would still exist — no rollback.",
        //    "(See Clip 5 for the pre-save vs post-save trade-off comparison.)"
        //}
        //));

        // Silent cleanup
        repo.Remove(savedOrder!);
        await repo.SaveAsync();
    }
}
