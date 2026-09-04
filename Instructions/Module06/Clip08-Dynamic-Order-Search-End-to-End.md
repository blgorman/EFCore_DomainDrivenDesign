# Clip08 - DEMO: Dynamic Order Search End to End
Unlock the five gated screens in `DemonstrateOrderSearch.cs`, then run the finished end-to-end demo: one named specification and two dynamic searches through the same `repository.ListAsync` call.

## Demo Class
- DemonstrateOrderSearch.RunEndToEndAsync

## Tests
No new tests are introduced in this clip.

## Files To Edit
- ConsoleAppProject/CodeAndDemonstrations/Module06/DemonstrateOrderSearch.cs

Note: prerequisite check — the screens below print real results only once `OrdersByCustomerSpecification`, `OrderSearchSpecification`, and `OrderRepository` (`Add`, `SaveAsync`, `ListAsync`) have been completed.

## What To Verify
- Screen 1 (database state): 8 orders total — 6 `Placed`, 1 `Shipped`, 1 `Cancelled`, 6 open.
- Screen 2 (named specification): `OrdersByCustomerSpecification(CustomerAId)` returns 4 orders, newest first, and `All returned orders have Lines.Count > 0: True`.
- Screen 3 (dynamic search, one filter): `OrderSearchSpecification(Placed, null)` returns 6 orders.
- Screen 4 (dynamic search, both filters): `OrderSearchSpecification(Shipped, 1)` returns 1 order (Bob's shipped three-line order).
- Screen 5 summary lists all three scenario counts, notes the repository call never changed, and closes on the module total: five specification classes, four fixed and one dynamic, against one unchanged `ListAsync`.
- The solution builds with zero compiler errors.

## Student Changes

Every step below is in ConsoleAppProject/CodeAndDemonstrations/Module06/DemonstrateOrderSearch.cs. Each of the five screens carries its own `Module 6 Clip 8` marker.

### 1. Unlock the seeded-data screen
- Find TODO marker: `Module 6 Clip 8 — remove this block and uncomment the seeded-data block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath, normalizing indentation as you go, so the screen reads:
  ```csharp
  var totalCount     = allOrders.Count;
  var placedCount    = allOrders.Count(o => o.Status == OrderStatus.Placed);
  var shippedCount   = allOrders.Count(o => o.Status == OrderStatus.Shipped);
  var cancelledCount = allOrders.Count(o => o.Status == OrderStatus.Cancelled);
  var openCount      = allOrders.Count(o => o.Status != OrderStatus.Shipped && o.Status != OrderStatus.Cancelled);

  Console.Write(OutputHelpers.BoxedArrayWithTitle(
      $"Database State — {totalCount} Orders Seeded",
      new[]
      {
          $"Total orders:     {totalCount}",
          $"  Placed:         {placedCount}",
          $"  Shipped:        {shippedCount}",
          $"  Cancelled:      {cancelledCount}",
          $"  Open (not shipped/cancelled): {openCount}"
      }
  ));
  ```

### 2. Unlock the named-specification screen
- Find TODO marker: `Module 6 Clip 8 — remove this block and uncomment the named-specification block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath, normalizing indentation as you go, so the screen reads:
  ```csharp
  var customerAOrderRows = customerAOrders.Select(o =>
      $"    Id={o.Id}  " +
      $"Status={o.Status,-10}  " +
      $"Lines={o.Lines.Count}  " +
      $"PlacedAt={o.PlacedAt:yyyy-MM-dd HH:mm}")
      .ToArray();

  Console.Write(OutputHelpers.BoxedArrayWithTitle(
      $"OrdersByCustomerSpecification — {customerAOrders.Count} result(s) for Customer {SeedDataHelper.CustomerAId}",
      new[]
      {
          $"Predicate: EF.Property<int>(o, \"CustomerId\") == {SeedDataHelper.CustomerAId}",
          "Include:   Lines (eager-loaded — Lines.Count reflects real data)",
          "Order:     Descending by PlacedAt",
          "---"
      }
      .Concat(customerAOrderRows)
      .Concat(new[]
      {
          "---",
          $"All returned orders have Lines.Count > 0: {customerAOrders.All(o => o.Lines.Count > 0)}"
      })
      .ToArray()
  ));
  ```

### 3. Unlock the one-filter search screen
- Find TODO marker: `Module 6 Clip 8 — remove this block and uncomment the one-filter search block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath, normalizing indentation as you go, so the screen reads:
  ```csharp
  var placedOrderRows = placedOrders.Select(o =>
      $"    Id={o.Id}  " +
      $"Status={o.Status,-10}  " +
      $"Lines={o.Lines.Count}  " +
      $"PlacedAt={o.PlacedAt:yyyy-MM-dd}")
      .ToArray();

  Console.Write(OutputHelpers.BoxedArrayWithTitle(
      $"OrderSearchSpecification(Placed, null) — {placedOrders.Count} result(s)",
      new[]
      {
          "status:   Placed  — WHERE o.Status == Placed",
          "minLines: null    — no line count filter",
          "---"
      }
      .Concat(placedOrderRows)
      .ToArray()
  ));
  ```

### 4. Unlock the both-filters search screen
- Find TODO marker: `Module 6 Clip 8 — remove this block and uncomment the both-filters search block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath, normalizing indentation as you go, so the screen reads:
  ```csharp
  var shippedOrderRows = shippedOrders.Any()
      ? shippedOrders.Select(o =>
          $"    Id={o.Id}  " +
          $"Status={o.Status,-10}  " +
          $"Lines={o.Lines.Count}  " +
          $"PlacedAt={o.PlacedAt:yyyy-MM-dd}")
          .ToArray()
      : new[] { "    (no results)" };

  Console.Write(OutputHelpers.BoxedArrayWithTitle(
      $"OrderSearchSpecification(Shipped, 1) — {shippedOrders.Count} result(s)",
      new[]
      {
          "status:   Shipped  — WHERE o.Status == Shipped",
          "minLines: 1        — AND o.Lines.Count >= 1",
          "---"
      }
      .Concat(shippedOrderRows)
      .ToArray()
  ));
  ```

### 5. Unlock the summary screen
- Find TODO marker: `Module 6 Clip 8 — remove this block and uncomment the summary block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath, normalizing indentation as you go, so the screen reads:
  ```csharp
  Console.Write(OutputHelpers.BoxedArrayWithTitle(
      "Results Summary — One Repository Method, Three Specifications",
      new[]
      {
          $"Named spec           OrdersByCustomerSpecification({SeedDataHelper.CustomerAId}):  {customerAOrders.Count} order(s)  — Lines eager-loaded",
          $"Dynamic, one filter  OrderSearchSpec(Placed, null):           {placedOrders.Count} order(s)  — status filter only",
          $"Dynamic, two filters OrderSearchSpec(Shipped, 1):             {shippedOrders.Count} order(s)  — both filters",
          "",
          "repository.ListAsync(spec) was called identically in all three scenarios.",
          "The specification was the only variable — the repository never changed.",
          "",
          "Zero .Where() calls in the demo. Zero LINQ in the caller.",
          "",
          "Module 6 wrote five specification classes in total.",
          "Four answer a fixed question. One answers whatever the caller asks.",
          "OrderRepository still exposes the single ListAsync method it started with."
      }
  ));
  ```
