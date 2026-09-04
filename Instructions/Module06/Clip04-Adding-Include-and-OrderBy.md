# Clip04 - Adding Include and OrderBy to Specifications
Add the `Include` and `OrderByDescending` declarations to `OrdersByCustomerSpecification`, then unlock both demo screens: the one that inspects the new declarations, and the one that runs the specification and shows the `JOIN` and `ORDER BY` in the SQL.

## Demo Class
- DemonstrateSpecificationWithIncludes.ShowIncludesAsync

## Tests
No new tests are introduced in this clip.

## Files To Edit
- OrderManagement.Infrastructure/Specifications/OrdersByCustomerSpecification.cs
- ConsoleAppProject/CodeAndDemonstrations/Module06/DemonstrateSpecificationWithIncludes.cs

Note: requires having completed the Clip 2 predicate.

## What To Verify
- Screen 1 prints `IncludeExpressions declared: 1` (expression targeting `o.Lines`) and `OrderExpressions declared: 1` (descending, by `PlacedAt`).
- Screen 2's SQL now contains a `JOIN` to `OrderLine` and an `ORDER BY ... DESC`, neither of which appeared in Clip 2's SQL.
- Screen 2 prints 4 orders for Customer A, newest first, and under each order its `OrderLine` rows with `ProductId`, `Quantity`, `UnitPrice`, and `LineTotal`, ending with `Lines.Count > 0 on every order: True`.
- The solution builds with zero compiler errors.

## Student Changes

### 1. Add Include and OrderBy to OrdersByCustomerSpecification
- File: OrderManagement.Infrastructure/Specifications/OrdersByCustomerSpecification.cs
- Find TODO marker: `Module 6 Clip 4 — Uncomment Include and OrderBy below:`
- Action: Delete the TODO comment and remove the `//` prefix from the line below it:
  ```csharp
  Query.Include(o => o.Lines).OrderByDescending(o => o.PlacedAt);
  ```

### 2. Unlock the internals screen
- File: ConsoleAppProject/CodeAndDemonstrations/Module06/DemonstrateSpecificationWithIncludes.cs
- Find TODO marker: `Module 6 Clip 4 — remove this block and uncomment the internals block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath (from `var includeCount` through the closing of the `BoxedArrayWithTitle` call), normalizing indentation as you go.

### 3. Unlock the execute screen
- File: ConsoleAppProject/CodeAndDemonstrations/Module06/DemonstrateSpecificationWithIncludes.cs
- Find TODO marker: `Module 6 Clip 4 — remove this block and uncomment the execute block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath (from `var sql` through the closing of the `BoxedArrayWithTitle` call), normalizing indentation as you go.
