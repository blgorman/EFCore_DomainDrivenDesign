using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Events;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateEventDispatcher
{
    public static async Task ShowDispatcherAsync(IServiceProvider serviceProvider)
    {
        // ── Step 1: How the dispatcher resolves handlers ───────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Clip 4 — Implementing a Simple Event Dispatcher"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "DomainEventDispatcher — Handler Resolution via Reflection",
            new[]
            {
                "DispatchAsync(IDomainEvent) resolves handlers through DI at runtime:",
                "",
                "  var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(event.GetType());",
                "  var handlers = _serviceProvider.GetServices(handlerType);",
                "  foreach (var handler in handlers)",
                "      await handler.HandleAsync(evt, ct);",
                "",
                "Each handler is registered against one closed generic interface:",
                "  services.AddScoped<IDomainEventHandler<OrderPlacedEvent>, OrderPlacedEventHandler>();",
                "",
                "MakeGenericType rebuilds that same closed type from the event's runtime type,",
                "so the dispatcher needs no switch statement and no reference to any handler class."
            }
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 2: Resolve every registered handler type via DI ───────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 2 — Resolve the Registered Handler for Every Event Type via DI"),
            ConsoleColor.DarkBlue);

        using var scope = serviceProvider.CreateScope();

        var eventTypes = new[]
        {
            typeof(OrderPlacedEvent),
            typeof(OrderShippedEvent),
            typeof(OrderCancelledEvent)
        };

        var resolutionLines = new List<string>();
        foreach (var eventType in eventTypes)
        {
            var closedHandlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            var resolvedHandlers = scope.ServiceProvider.GetServices(closedHandlerType).ToList();

            resolutionLines.Add($"  IDomainEventHandler<{eventType.Name}>  ->  {resolvedHandlers.Count} registration(s)");
            foreach (var resolvedHandler in resolvedHandlers)
            {
                resolutionLines.Add($"      {resolvedHandler?.GetType().Name ?? "(null)"}");
            }
        }

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "MakeGenericType + GetServices — One Call Site, Three Event Types",
            new[]
            {
                "The same two lines of reflection resolve a different handler for each event type:",
                ""
            }
            .Concat(resolutionLines)
            .Concat(new[]
            {
                "",
                "No handler class is named anywhere above. The event's runtime type selects it."
            })
            .ToArray()
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 3: Dispatch a real OrderShippedEvent before the save ──────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 3 — Manually Dispatch a Real OrderShippedEvent"),
            ConsoleColor.DarkBlue);

        //TODO: Module 5 Clip 4 — Delete this Not Yet Implemented box:
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This demo will work after completing Module 5 Clip 4.",
                "Open DemonstrateEventDispatcher.cs and uncomment the //TODO: Module 5 Clip 4 block.",
                "Prerequisite: DomainEventDispatcher.DispatchAsync and OrderShippedEventHandler.HandleAsync."
            }
        ));

        //TODO: Module 5 Clip 4 — Uncomment the Step 3 dispatch block below:
        //var orderingContext = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        //await SeedDataHelper.EnsureCustomersAsync(orderingContext);
        //
        //var order = Order.Place(
        //    SeedDataHelper.CustomerAId,
        //new[] { (SeedDataHelper.Product1Id, 2, Money.Create(29.99m, "USD")) });
        //
        //orderingContext.Orders.Add(order);
        //orderingContext.Entry(order).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
        //await orderingContext.SaveChangesAsync();
        //
        //order.Process();
        //order.Confirm();
        //order.Ship();
        //
        //var shippedEvent = order.DomainEvents.OfType<OrderShippedEvent>().Single();
        //
        //var handlerOutput = new List<string>();
        //using var dispatchProvider = CreateCapturingDispatchProvider(orderingContext, handlerOutput);
        //
        //using (var dispatchScopeBeforeSave = dispatchProvider.CreateScope())
        //{
        //var dispatcherBeforeSave = dispatchScopeBeforeSave.ServiceProvider.GetRequiredService<DomainEventDispatcher>();
        //await dispatcherBeforeSave.DispatchAsync(shippedEvent);
        //}
        //
        //var statusBeforeSave = await ReadProjectionStatusAsync(dispatchProvider, shippedEvent.OrderId);
        //var handlerLineBeforeSave = handlerOutput.Count > 0 ? handlerOutput[^1] : NoHandlerOutput;
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //"Dispatched Before Save",
        //new[]
        //{
        //    $"OrderId:                      {shippedEvent.OrderId}",
        //    $"Aggregate Status (in memory): {order.Status}",
        //    $"Orders table Status (query):  {statusBeforeSave}",
        //    "",
        //    "Handler output:",
        //    $"  {handlerLineBeforeSave}"
        //}
        //));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 4: Save, then dispatch the same event from a second scope ─────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 4 — Save the Ship Transition, Then Dispatch the Same Event Again"),
            ConsoleColor.DarkBlue);

        //TODO: Module 5 Clip 4 — Delete this Not Yet Implemented box:
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This demo will work after completing Module 5 Clip 4.",
                "Open DemonstrateEventDispatcher.cs and uncomment the //TODO: Module 5 Clip 4 block.",
                "Prerequisite: DomainEventDispatcher.DispatchAsync and OrderShippedEventHandler.HandleAsync."
            }
        ));

        //TODO: Module 5 Clip 4 — Uncomment the Step 4 dispatch block below:
        //await orderingContext.SaveChangesAsync();
        //
        //using (var dispatchScopeAfterSave = dispatchProvider.CreateScope())
        //{
        //var dispatcherAfterSave = dispatchScopeAfterSave.ServiceProvider.GetRequiredService<DomainEventDispatcher>();
        //await dispatcherAfterSave.DispatchAsync(shippedEvent);
        //}
        //
        //var statusAfterSave = await ReadProjectionStatusAsync(dispatchProvider, shippedEvent.OrderId);
        //var handlerLineAfterSave = handlerOutput.Count > 0 ? handlerOutput[^1] : NoHandlerOutput;
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //"Dispatched After Save",
        //new[]
        //{
        //    $"OrderId:                      {shippedEvent.OrderId}",
        //    $"Aggregate Status (in memory): {order.Status}",
        //    $"Orders table Status (query):  {statusAfterSave}",
        //    "",
        //    "Handler output:",
        //    $"  {handlerLineAfterSave}"
        //}
        //));
    }

    private const string NoHandlerOutput = "(handler produced no output)";

    //TODO: Module 5 Clip 4 — Uncomment the projection reader below:
    //private static async Task<string> ReadProjectionStatusAsync(ServiceProvider provider, int orderId)
    //{
    //using var scope = provider.CreateScope();
    //var shippingContext = scope.ServiceProvider.GetRequiredService<ShippingContext>();
    //
    //var row = await shippingContext.Shipments
    //    .AsNoTracking()
    //    .FirstOrDefaultAsync(s => s.Id == orderId);
    //
    //return row is null ? "(no row)" : row.Status.ToString();
    //}

    private static ServiceProvider CreateCapturingDispatchProvider(OrderingContext source, List<string> handlerOutput)
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(LogCaptureHelpers.CaptureFor(nameof(OrderShippedEventHandler), handlerOutput));
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddDbContext<ShippingContext>(options =>
            options.UseSqlServer(source.Database.GetConnectionString()));

        services.AddScoped<IDomainEventHandler<OrderShippedEvent>, OrderShippedEventHandler>();
        services.AddScoped<DomainEventDispatcher>();

        return services.BuildServiceProvider();
    }

}
