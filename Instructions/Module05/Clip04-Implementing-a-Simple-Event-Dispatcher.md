# Clip04 - Implementing a Simple Event Dispatcher
Implement `DomainEventDispatcher.DispatchAsync` and the three event handlers, then unlock the demo's two dispatch screens — this completes the pipeline the Clip 3 interceptor feeds into.

Module 5 depends on all previous modules having been successfully implemented.

## Demo Class
- DemonstrateEventDispatcher.ShowDispatcherAsync

## Tests
No unit tests are introduced in this clip.

## Files To Edit
- OrderManagement.Infrastructure/Events/DomainEventDispatcher.cs
- OrderManagement.Infrastructure/Events/OrderPlacedEventHandler.cs
- OrderManagement.Infrastructure/Events/OrderCancelledEventHandler.cs
- OrderManagement.Infrastructure/Events/OrderShippedEventHandler.cs
- ConsoleAppProject/CodeAndDemonstrations/Module05/DemonstrateEventDispatcher.cs

This clip requires having completed Clip 3 (`DomainEventDispatchInterceptor.SavedChangesAsync`) for the full pipeline to fire on saves, though the manual dispatch in this demo works with the dispatcher and handlers alone.

## What To Verify
- Screen 2 lists all three closed handler interfaces — `IDomainEventHandler<OrderPlacedEvent>`, `IDomainEventHandler<OrderShippedEvent>`, and `IDomainEventHandler<OrderCancelledEvent>` — each reporting one registration and resolving its matching handler class.
- Screen 3 prints four values: the real `OrderId`, `Aggregate Status (in memory): Shipped`, `Orders table Status (query): Placed`, and the handler's own output line reporting `Status=Placed`.
- Screen 4 prints the same four values with `Orders table Status (query): Shipped` and a handler output line reporting `Status=Shipped`. The aggregate status and OrderId are unchanged from Screen 3.
- Every value on both screens comes from a live read. The handler's text is captured from the handler itself, not retyped by the demo.
- The demo is four screens, one box each, and stops for input three times.
- No "Not Yet Implemented" box prints anywhere.
- The solution builds with zero compiler errors.

## Student Changes

### 1. Implement DispatchAsync
- File: OrderManagement.Infrastructure/Events/DomainEventDispatcher.cs
- Find TODO marker: `Module 5 Clip 4 — Uncomment the implementation below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it (the `handlerType` closed-generic construction, the `GetServices` resolution, and the handler loop). Then delete the now-redundant `await Task.CompletedTask;` at the end of the method — the uncommented loop provides the method's awaits.

### 2. Log in OrderPlacedEventHandler
- File: OrderManagement.Infrastructure/Events/OrderPlacedEventHandler.cs
- Find TODO marker: `Module 5 Clip 4 — Uncomment the log line below:`
- Action: Delete the TODO comment and uncomment the line below it:
  ```csharp
  _logger.LogInformation("Order placed: {OrderId}", domainEvent.OrderId);
  ```
  Keep the existing `await Task.CompletedTask;`. The method is `async Task`, and a `LogInformation` call has nothing to await, so that line is what keeps the compiler from warning.

### 3. Log in OrderCancelledEventHandler
- File: OrderManagement.Infrastructure/Events/OrderCancelledEventHandler.cs
- Find TODO marker: `Module 5 Clip 4 — Uncomment the log line below:`
- Action: Delete the TODO comment and uncomment the line below it:
  ```csharp
  _logger.LogInformation("Order cancelled: {OrderId} at {CancelledAt}", domainEvent.OrderId, domainEvent.CancelledAt);
  ```
  Keep the existing `await Task.CompletedTask;`, for the same reason as the placed handler.

### 4. Read the shipping projection in OrderShippedEventHandler
- File: OrderManagement.Infrastructure/Events/OrderShippedEventHandler.cs
- Find TODO marker: `Module 5 Clip 4 — Uncomment the implementation below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it (the `Shipments` query and the `LogInformation`/`LogWarning` branches). Then delete the now-redundant `await Task.CompletedTask;` at the end of the method — the uncommented query provides the method's await.
- Note the design comment at the top of the file: this handler injects `ShippingContext` directly because `ShipmentReadModel` is a read projection, not an aggregate root — the repository pattern is reserved for aggregate roots.

Complete steps 5 and 6 in order. The Step 4 block uses `orderingContext`, `dispatchProvider`, and `shippedEvent`, all of which are declared in the Step 3 block, so Step 3 has to be uncommented first for the solution to build.

### 5. Uncomment the projection reader
- File: `ConsoleAppProject/CodeAndDemonstrations/Module05/DemonstrateEventDispatcher.cs`
- Find TODO marker: `Module 5 Clip 4 — Uncomment the projection reader below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line of the method beneath it, so the method reads:
  ```csharp
  private static async Task<string> ReadProjectionStatusAsync(ServiceProvider provider, int orderId)
  {
      using var scope = provider.CreateScope();
      var shippingContext = scope.ServiceProvider.GetRequiredService<ShippingContext>();

      var row = await shippingContext.Shipments
          .AsNoTracking()
          .FirstOrDefaultAsync(s => s.Id == orderId);

      return row is null ? "(no row)" : row.Status.ToString();
  }
  ```
- The two screens below call this method to read the shipping projection. It ships commented because `ShippingContext.Shipments` does not exist until Module 3 Clip 4 adds it, and the solution has to compile before that.

### 6. Unlock the Step 3 dispatch screen
- File: ConsoleAppProject/CodeAndDemonstrations/Module05/DemonstrateEventDispatcher.cs
- Find TODO marker: `Module 5 Clip 4 — Delete this Not Yet Implemented box:` in the Step 3 section
- Action: Delete the TODO comment and the active `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call below it.
- Find TODO marker: `Module 5 Clip 4 — Uncomment the Step 3 dispatch block below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it, normalizing indentation as you go. The block creates the order, saves it, runs the three status transitions, pulls the `OrderShippedEvent` off the aggregate, builds the console logging provider, prints the dispatch box, and calls `DispatchAsync`.

### 7. Unlock the Step 4 dispatch screen
- File: same
- Find TODO marker: `Module 5 Clip 4 — Delete this Not Yet Implemented box:` in the Step 4 section
- Action: Delete the TODO comment and the active `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call below it.
- Find TODO marker: `Module 5 Clip 4 — Uncomment the Step 4 dispatch block below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it, normalizing indentation as you go. The block saves the ship transition, opens a second DI scope, prints the second dispatch box, and calls `DispatchAsync` with the same event instance.
