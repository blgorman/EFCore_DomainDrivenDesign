# Clip05 - Loading the Full Aggregate: Include Strategy
Loads the same seeded order across five screens and prints real results each time. `Include(o => o.Lines)` returns the full aggregate; omitting `Include` returns the same order with `Lines.Count` of zero while the rows still sit in the database; `UnitPrice` arrives populated because `Money` is an owned entity mapped into columns on the `OrderLine` row; and `AsSplitQuery` returns the identical aggregate through separate round trips instead of one join.

## Demo Class
- DemonstrateAggregateLoading.ShowIncludeStrategyAsync

## Tests
No unit tests are introduced in this clip.

## Files To Edit
- ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstrateAggregateLoading.cs

This clip has no prerequisite. The demo queries `OrderingContext` directly and never calls `IOrderRepository`, so it runs before Clip 4 is implemented.

## What To Verify
- Scenario A prints an `Order` loaded with `.Include(o => o.Lines)` — `Lines.Count` matches the seeded line count, and each line's `UnitPrice` is populated.
- Scenario B prints the same order loaded from a fresh scope with no `Include` — `Lines.Count` is `0` even though the rows exist in the database.
- The owned-entity section reuses `orderWithLines.Lines.First().UnitPrice` from Scenario A as proof that `Money` loads from columns on the `OrderLine` row, with Scenario A's query writing one `Include` and writing nothing for `UnitPrice`.
- The `AsSplitQuery()` section runs a raw `COUNT(*)` over the `Orders`-to-`OrderLine` join to print how many rows that join returns for one order, then reloads the same order with `AsSplitQuery()` and prints an identical `Lines.Count` to Scenario A.
- The generated-SQL screen prints one `SELECT` containing `LEFT JOIN [OrderLine]`, then after a keypress, two separate `SELECT` statements for the `AsSplitQuery` load.
- No "Not Yet Implemented" box prints anywhere in the output.

## Student Changes

### 1. Delete the "Not Yet Implemented" box
- File: ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstrateAggregateLoading.cs
- Method: ShowIncludeStrategyAsync
- Find TODO marker: `Module 4 Clip 5 — Delete the "Not Yet Implemented" box below.`
- Action: Delete the TODO comment and the `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call directly below it. Leave the `SECTION E` banner above the TODO in place.

### 2. Uncomment the generated-SQL block
- File: same
- Find TODO marker: `Module 4 Clip 5 — Uncomment this block to print the generated SQL.`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it. `CreateLoggingContext`, at the bottom of the file, is already written and needs no edit — it builds an `OrderingContext` with `.LogTo(Console.WriteLine, ...)` filtered to `DbLoggerCategory.Database.Command`, so EF Core writes each statement it executes to the console.
