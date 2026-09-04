using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Events;
using OrderManagement.Infrastructure.Repositories;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateOrderPlacedEvent
{
    public static async Task RunEndToEndAsync(IServiceProvider serviceProvider)
    {
        string connectionString;
        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(seedCtx);
            connectionString = seedCtx.Database.GetConnectionString()!;
        }

        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Clip 6 — DEMO: OrderPlaced Event Complete Workflow"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderPlacedEvent Workflow — 5 Stages",
            new[]
            {
                "Stage 1: Event CREATED — Order.Place() calls RaiseDomainEvent()",
                "Stage 2: Event QUEUED on the aggregate root until SaveChanges",
                "Stage 3: INTERCEPTOR fires after SaveChanges commits",
                "Stage 4: HANDLER runs — side effect logged via ILogger",
                "Stage 5: Events CLEARED from the aggregate"
            }
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Stage 1: Create the order ─────────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Stage 1 — Order.Place() Raises OrderPlacedEvent"),
            ConsoleColor.DarkBlue);

        var handlerOutput = new List<string>();
        using var dispatchProvider = CreateCapturingProvider(connectionString, handlerOutput);
        using var dispatchScope = dispatchProvider.CreateScope();

        var context = dispatchScope.ServiceProvider.GetRequiredService<OrderingContext>();
        var repo = dispatchScope.ServiceProvider.GetRequiredService<IOrderRepository>();

        var order = Order.Place(
            SeedDataHelper.CustomerAId,
            new[] { (SeedDataHelper.Product1Id, 2, Money.Create(29.99m, "USD")) }
        );

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Stage 1 — Aggregate Created, Event Queued",
            new[]
            {
                $"Order Id:       {order.Id}",
                $"Status:         {order.Status}",
                $"PlacedAt:       {order.PlacedAt:u}",
                $"Lines:          {order.Lines.Count}",
                $"Events queued:  {order.DomainEvents.Count}"
            }
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Stage 2: Inspect DomainEvents on the aggregate ────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Stage 2 — Inspect DomainEvents on the Aggregate"),
            ConsoleColor.DarkBlue);

        var queuedEvent = order.DomainEvents[0];
        var queuedPlacedEvent = (OrderPlacedEvent)queuedEvent;

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Stage 2 — The Queued Event and the Id It Carries",
            new[]
            {
                $"Events queued:             {order.DomainEvents.Count}",
                $"Event type:                {queuedEvent.GetType().Name}",
                $"OrderPlacedEvent.OrderId:  {queuedPlacedEvent.OrderId}",
                $"OrderPlacedEvent.PlacedAt: {queuedPlacedEvent.PlacedAt:u}",
                "",
                "Order.Place() raises the event before any INSERT, so the copied id is 0.",
                "OrderPlacedEvent is a record, so the 0 stays after the INSERT assigns an id.",
                "Order.Ship() runs after the save, so OrderShippedEvent carries a real id."
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

        var savedOrder = await context.Orders.FindAsync(order.Id);

        // ── Stage 3: SaveChangesAsync committed the row ───────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Stage 3 — SaveChangesAsync Committed the Row"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Stage 3 — Row Committed to the Database",
            new[]
            {
                $"DB row found via FindAsync: {savedOrder is not null}",
                $"Saved row Id:               {savedOrder?.Id}"
            }
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Stage 4: Handler ran — its own output ─────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Stage 4 — Handler Ran: Side Effect Logged"),
            ConsoleColor.DarkBlue);

        //TODO: Module 5 Clip 6 — Delete this Not Yet Implemented box:
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This demo will work after completing Module 5 Clip 6.",
                "Open DemonstrateOrderPlacedEvent.cs and uncomment the Stage 4 block below.",
                "Prerequisite: requires having completed Module 5 Clip 3 and Clip 4 — interceptor, dispatcher, and handlers."
            }
        ));

        //TODO: Module 5 Clip 6 — Uncomment the Stage 4 handler box below:
        //var handlerLine = handlerOutput.Count > 0 ? handlerOutput[^1] : NoHandlerOutput;
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "Stage 4 — Handler Ran Against a Committed Row",
        //    new[]
        //    {
        //        $"Saved row Id:              {savedOrder?.Id}",
        //        $"OrderPlacedEvent.OrderId:  {queuedPlacedEvent.OrderId}",
        //        "",
        //        "Handler output:",
        //        $"  {handlerLine}"
        //    }
        //));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Stage 5: Events cleared from the aggregate ────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Stage 5 — Events Cleared from the Aggregate"),
            ConsoleColor.DarkBlue);

        //TODO: Module 5 Clip 6 — Delete this Not Yet Implemented box:
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This demo will work after completing Module 5 Clip 6.",
                "Open DemonstrateOrderPlacedEvent.cs and uncomment the Stage 5 block below.",
                "Prerequisite: requires having completed Module 5 Clip 3 — DomainEventDispatchInterceptor.SavedChangesAsync."
            }
        ));

        //TODO: Module 5 Clip 6 — Uncomment the Stage 5 cleared-events box below:
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "Stage 5 — Aggregate Is Clean",
        //    new[]
        //    {
        //        $"Events remaining on aggregate: {order.DomainEvents.Count}"
        //    }
        //));

        // Silent cleanup
        if (savedOrder is not null)
        {
            repo.Remove(savedOrder);
            await repo.SaveAsync();
        }
    }

    private const string NoHandlerOutput = "(handler produced no output)";

    // Mirrors the domain event registrations in Program.cs, with the handler's logger
    // routed into a list so the demo can print what the handler wrote inside a box.
    private static ServiceProvider CreateCapturingProvider(string connectionString, List<string> handlerOutput)
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(LogCaptureHelpers.CaptureFor(nameof(OrderPlacedEventHandler), handlerOutput));
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddScoped<DomainEventDispatcher>();
        services.AddScoped<DomainEventDispatchInterceptor>();
        services.AddScoped<IDomainEventHandler<OrderPlacedEvent>, OrderPlacedEventHandler>();

        services.AddDbContext<OrderingContext>((sp, options) =>
            options.UseSqlServer(connectionString)
                   .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IOrderRepository, OrderRepository>();

        return services.BuildServiceProvider();
    }
}
