# Clip04 - Implementing the Repository in Infrastructure
Turns `OrderRepository` from six stubs into a working class, then proves how the repository and its `OrderingContext` are wired. Reflection shows `_ctx` is private and readonly with no public way to replace it. Adding an order to a raw `OrderingContext` and reading the count back through the repository's own `_ctx` shows dependency injection handed both the same instance. Two resolutions inside one scope compare equal by reference, while a second scope sees none of the first scope's unsaved work. Ends with a live `GetByIdAsync` against seeded data.

## Demo Class
- DemonstrateOrderRepository.WalkImplementationAsync

## Tests
No unit tests are introduced in this clip. `OrderRepositoryTests.cs` already exists and is not edited here, but you run it twice: once before you start, to watch `Add_ThenGetById_ReturnsOrderWithLines` fail against the stubs, and once at the end, to watch it pass. Step 0 and Step 9 below.

The test needs Docker running, because `MsSqlFixture` starts a real SQL Server container.

## Files To Edit
- OrderManagement.Infrastructure/Repositories/OrderRepository.cs
- OrderManagement.Application/Handlers/PlaceOrderHandler.cs
- ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstrateOrderRepository.cs

Reading `OrderRepository.cs` top to bottom, there are five TODO markers:
1. `Module 4 Clip 4 — Implement GetByIdAsync with Include so the full aggregate loads:`
2. `Module 4 Clip 4 — Implement Add and Remove (change-tracking only, no SQL issued yet):`
3. `Module 4 Clip 4 — Implement the Add overload that sets the shadow CustomerId FK:`
4. `Module 4 Clip 4 — Implement SaveAsync to flush all pending ChangeTracker entries:`
5. `Module 4 Clip 4 — Implement ListAsync using SpecificationEvaluator:`

`PlaceOrderHandler.cs` has one TODO marker. `DemonstrateOrderRepository.cs` has two TODO markers (Proof 3b and the live `GetByIdAsync` call in Part 3).

## What To Verify
- Proof 3b prints `ChangeTracker.Entries().Count()` as `1` for scope A and `0` for scope B — scope B cannot see scope A's unsaved `Add`.
- The live `GetByIdAsync` call in Part 3 prints a real seeded order's `Id`, `CustomerId` (via the shadow-property accessor), `Status`, `PlacedAt`, and a non-zero `Lines.Count`.
- `PlaceOrderHandler.HandleAsync` returns a non-zero `order.Id` and the order is actually present in the database afterward (not just held in memory).
- The solution builds with zero compiler errors — every uncommented block must compile against the real `Order` aggregate, including the shadow `CustomerId` property.

## Student Changes

### 0. Run the tests and watch one fail
- Terminal at the solution root:
  ```powershell
  dotnet test --filter Module=4
  ```
- Expected result: **1 passed, 1 failed.**
- `Add_ThenGetById_ReturnsOrderWithLines` fails because `OrderRepository.Add` is an empty method body and `GetByIdAsync` returns `null`. Nothing is written, so nothing reloads.
- `Remove_ThenGetById_ReturnsNull` passes for the wrong reason: `GetByIdAsync` returns `null` no matter what, so a test asserting `null` is satisfied by the stub.
- Leave this output on screen. Steps 1 through 8 are what turn it green.

### 1. Implement GetByIdAsync
- File: OrderManagement.Infrastructure/Repositories/OrderRepository.cs
- Find TODO marker: `Module 4 Clip 4 — Implement GetByIdAsync with Include so the full aggregate loads:`
- Action: Delete the TODO comment, its commented example, and the active stub below it:
  ```csharp
  public Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
      => Task.FromResult<Order?>(null);
  ```
  Replace with:
  ```csharp
  public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
  {
      return await _ctx.Orders
          .Include(o => o.Lines)
          .FirstOrDefaultAsync(o => o.Id == id, ct);
  }
  ```

### 2. Implement Add and Remove
- File: same
- Find TODO marker: `Module 4 Clip 4 — Implement Add and Remove (change-tracking only, no SQL issued yet):`
- Action: Delete the TODO comment, its commented example, and the active stub below it:
  ```csharp
  public void Add(Order order) { }
  public void Remove(Order order) { }
  ```
  Replace with:
  ```csharp
  public void Add(Order order) => _ctx.Orders.Add(order);
  public void Remove(Order order) => _ctx.Orders.Remove(order);
  ```

### 3. Implement the Add overload that sets the shadow CustomerId FK
- File: same
- Find TODO marker: `Module 4 Clip 4 — Implement the Add overload that sets the shadow CustomerId FK:`
- Action: Delete the TODO comment, its commented example, and the active stub below it:
  ```csharp
  public void Add(Order order, int customerId) { }
  ```
  Replace with:
  ```csharp
  public void Add(Order order, int customerId)
  {
      _ctx.Orders.Add(order);
      _ctx.Entry(order).Property<int>("CustomerId").CurrentValue = customerId;
  }
  ```
  `Order.Place()` validates the customer id but no longer stores it — `CustomerId` became a pure shadow
  property in Module 2 Clip 5. This overload is the one place that holds both the aggregate and the
  context, so it sets the shadow FK exactly the way `SeedDataHelper` already does.

### 4. Implement SaveAsync
- File: same
- Find TODO marker: `Module 4 Clip 4 — Implement SaveAsync to flush all pending ChangeTracker entries:`
- Action: Delete the TODO comment, its commented example, and the active stub below it:
  ```csharp
  public Task SaveAsync(CancellationToken ct = default) => Task.CompletedTask;
  ```
  Replace with:
  ```csharp
  public async Task SaveAsync(CancellationToken ct = default)
  {
      await _ctx.SaveChangesAsync(ct);
  }
  ```

### 5. Implement ListAsync
- File: same
- Find TODO marker: `Module 4 Clip 4 — Implement ListAsync using SpecificationEvaluator:`
- Action: Delete the TODO comment, its commented example, and the active stub below it:
  ```csharp
  public Task<IReadOnlyList<Order>> ListAsync(ISpecification<Order> spec, CancellationToken ct = default)
      => Task.FromResult<IReadOnlyList<Order>>(Array.Empty<Order>());
  ```
  Replace with:
  ```csharp
  public async Task<IReadOnlyList<Order>> ListAsync(ISpecification<Order> spec, CancellationToken ct = default)
  {
      var query = SpecificationEvaluator.Default.GetQuery(_ctx.Orders.AsQueryable(), spec);
      return await query.ToListAsync(ct);
  }
  ```

### 6. Wire up PlaceOrderHandler
- File: OrderManagement.Application/Handlers/PlaceOrderHandler.cs
- Find TODO marker: `Module 4 Clip 4 — Uncomment after implementing the IOrderRepository.Add(order, customerId) overload and SaveAsync.`
- Action: Delete the TODO comment and uncomment the two lines below it:
  ```csharp
  _repo.Add(order, cmd.CustomerId);
  await _repo.SaveAsync(ct);
  ```
  The handler holds only `IOrderRepository`, never `OrderingContext`, so it passes `cmd.CustomerId`
  into the overload and lets the repository set the shadow FK. The persistence detail stays behind the interface.

### 7. Unlock Proof 3b in DemonstrateOrderRepository.cs
- File: ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstrateOrderRepository.cs
- Find TODO marker: `Module 4 Clip 4 — Delete the Proof 3b "Not Yet Implemented" box below.`
- Action: Delete the TODO comment and the `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call directly below it. Leave the section banner line above the TODO in place.
- Find TODO marker: `Module 4 Clip 4 — Uncomment after implementing IOrderRepository.Add:`
- Action: Delete the TODO comment, then remove the `//` prefix from every line in the block below it (the `scopeA` and `scopeB` block, through its closing brace). The `WaitForUserInput` separator already sitting below this block stays where it is.

### 8. Unlock the live GetByIdAsync call in DemonstrateOrderRepository.cs
- File: same
- Find TODO marker: `Module 4 Clip 4 — Delete the Live Demo "Not Yet Implemented" box below.`
- Action: Delete the TODO comment and the `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call directly below it. Leave the section banner line above the TODO in place.
- Find TODO marker: `Module 4 Clip 4 — Uncomment after implementing IOrderRepository.GetByIdAsync:`
- Action: Delete the TODO comment, then remove the `//` prefix from every line in the block below it (the live `GetByIdAsync` call and its output box). This is the last block in the method; nothing follows it, so no `WaitForUserInput` separator belongs after it.

### 9. Run the tests again and watch the failure turn green
- Terminal at the solution root:
  ```powershell
  dotnet test --filter Module=4
  ```
- Expected result: **2 passed, 0 failed.**
- `Add_ThenGetById_ReturnsOrderWithLines` now passes because `Add` stages the order, `SaveAsync` writes it, and `GetByIdAsync` reloads it with `Lines` included.
- `Remove_ThenGetById_ReturnsNull` still passes, and now for the right reason: the order really was written, really was removed, and `GetByIdAsync` really queried the database before returning `null`.
- Compare against the Step 0 output. The same two tests, the same command, one implementation apart.
