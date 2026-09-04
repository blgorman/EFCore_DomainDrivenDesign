# Clip06 - Dynamic Specification Composition at Runtime
Implement the conditional filters in `OrderSearchSpecification`, then unlock every Clip 6-gated screen in the composition demo. Four fixed specification classes exist at this point, and covering optional filters with more fixed classes multiplies them, so one constructor takes the optional inputs instead.

## Demo Class
- DemonstrateDynamicSpecComposition.ShowCompositionAsync

## Tests
- `OrderManagement.Tests/Application/SpecificationTests.cs` → `OrderSearchSpec_WithStatusFilter_FiltersCorrectly` and `OrderSearchSpec_WithNoFilters_ReturnsAll` (Trait Module 6, Clip 6) pass once this clip's spec implementation is in place. They are walked in Clip 7.

## Files To Edit
- OrderManagement.Infrastructure/Specifications/OrderSearchSpecification.cs
- ConsoleAppProject/CodeAndDemonstrations/Module06/DemonstrateDynamicSpecComposition.cs

## What To Verify
- The no-filter search (`null, null`) returns all 8 orders; the status-only search (`Placed, null`) returns 6; the both-filters search (`Placed, 2`) returns 2 — the two placed orders carrying more than one line.
- Screen 4's summary box lists all three search counts side by side.
- The solution builds with zero compiler errors.

## Student Changes

### 1. Implement OrderSearchSpecification
- File: OrderManagement.Infrastructure/Specifications/OrderSearchSpecification.cs
- Find TODO marker: `Module 6 Clip 6 — Uncomment the implementation below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it:
  ```csharp
  if (status.HasValue)
      Query.Where(o => o.Status == status.Value);

  if (minLines.HasValue)
      Query.Where(o => o.Lines.Count >= minLines.Value);

  Query.Include(o => o.Lines).OrderByDescending(o => o.PlacedAt);
  ```

Steps 2 through 5 are all in ConsoleAppProject/CodeAndDemonstrations/Module06/DemonstrateDynamicSpecComposition.cs. Each of the four screens carries its own `Module 6 Clip 6` marker.

### 2. Unlock the no-filters screen
- Find TODO marker: `Module 6 Clip 6 — remove this block and uncomment the no-filters block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath, normalizing indentation as you go, so the screen reads:
  ```csharp
  Console.Write(OutputHelpers.BoxedArrayWithTitle(
      "OrderSearchSpecification(null, null) — No Filters Active",
      new[]
      {
          "status:   null  — no Status WHERE clause added",
          "minLines: null  — no line count WHERE clause added",
          "",
          $"Orders returned: {allOrders.Count}  (all orders in database)",
          "",
          "The spec still ran through the evaluator — it just had no Where clauses.",
          "No branching in the repository. No special-case code for the empty filter."
      }
  ));
  ```

### 3. Unlock the status-filter screen
- Find TODO marker: `Module 6 Clip 6 — remove this block and uncomment the status-filter block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath, normalizing indentation as you go, so the screen reads:
  ```csharp
  var placedOrderRows = placedOrders.Select(o =>
      $"    Id={o.Id}  Status={o.Status,-10}  PlacedAt={o.PlacedAt:yyyy-MM-dd}")
      .ToArray();

  Console.Write(OutputHelpers.BoxedArrayWithTitle(
      $"OrderSearchSpecification(Placed, null) — {placedOrders.Count} result(s)",
      new[]
      {
          "status:   Placed  — WHERE o.Status == Placed added",
          "minLines: null    — no line count filter",
          "---"
      }
      .Concat(placedOrderRows)
      .Concat(new[]
      {
          "---",
          "Only one .Where() clause was added — the minLines clause was skipped."
      })
      .ToArray()
  ));
  ```

### 4. Unlock the both-filters screen
- Find TODO marker: `Module 6 Clip 6 — remove this block and uncomment the both-filters block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath, normalizing indentation as you go, so the screen reads:
  ```csharp
  var placedMultiLineOrderRows = placedMultiLineOrders.Select(o =>
      $"    Id={o.Id}  Status={o.Status,-10}  Lines={o.Lines.Count}  PlacedAt={o.PlacedAt:yyyy-MM-dd}")
      .ToArray();

  Console.Write(OutputHelpers.BoxedArrayWithTitle(
      $"OrderSearchSpecification(Placed, 2) — {placedMultiLineOrders.Count} result(s)",
      new[]
      {
          "status:   Placed  — WHERE o.Status == Placed",
          "minLines: 2       — AND o.Lines.Count >= 2",
          "---"
      }
      .Concat(placedMultiLineOrderRows)
      .Concat(new[]
      {
          "---",
          "Both .Where() clauses were added — the evaluator ANDed them together.",
          "One spec class handled all three scenarios with no repository changes."
      })
      .ToArray()
  ));
  ```

### 5. Unlock the benefits-summary screen
- Find TODO marker: `Module 6 Clip 6 — remove this block and uncomment the benefits-summary block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath, normalizing indentation as you go, so the screen reads:
  ```csharp
  Console.Write(OutputHelpers.BoxedArrayWithTitle(
      "Dynamic Composition — Key Benefits",
      new[]
      {
          $"No filters:               {allOrders.Count} orders — no WHERE added",
          $"Status only:              {placedOrders.Count} orders — 1 WHERE added",
          $"Status + min lines:       {placedMultiLineOrders.Count} orders — 2 WHEREs added",
          "",
          "One specification class handled all three combinations.",
          "repository.ListAsync(spec) is the same call in every scenario."
      }
  ));
  ```
