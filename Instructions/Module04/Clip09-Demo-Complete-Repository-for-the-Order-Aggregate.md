# Clip09 - DEMO: Complete Repository for the Order Aggregate
Runs one `Order` through its full lifecycle across six steps, every step going through `IOrderRepository`. `Order.Place` builds the aggregate in memory with nothing in the database, `Add` and `SaveAsync` persist it and return the generated `Id`, a fresh scope reloads the full graph because `GetByIdAsync` bakes in `.Include(o => o.Lines)`, `AddLine` mutates through the Aggregate Root and re-persists, and a third scope reloads and prints every line including the new one. Step 6 then places a second order through `PlaceOrderHandler`, the application-layer caller the API uses, to show the same repository driven by a real consumer rather than by demo code.

## Demo Class
- DemonstrateRepositoryEndToEnd.RunEndToEndAsync

## Tests
No unit tests are introduced in this clip.

## Files To Edit
- ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstrateRepositoryEndToEnd.cs

This clip requires having completed Clip 4 (`OrderRepository`'s `Add`, `SaveAsync`, `GetByIdAsync`, and `Remove` must already be implemented) — without it, the code below won't behave as described.

## What To Verify
- Step 1 prints the newly created `Order` with `Lines.Count` matching the two seeded lines, before anything is persisted.
- Step 2 prints a non-zero generated `Id` after `writeRepo.Add(order, SeedDataHelper.CustomerAId)` + `writeRepo.SaveAsync()`.
- Step 3 reloads the order through a fresh scope and prints the same `Lines.Count`, proving `GetByIdAsync`'s baked-in `Include` works end to end.
- Step 4 prints `Lines` count before and after `loaded.AddLine(...)` + `updateRepo.SaveAsync()` — the count increases by one.
- Step 5 reloads the order through yet another fresh scope and prints the complete final line list, including the line added in Step 4.
- Step 6 resolves `PlaceOrderHandler`, hands it a `PlaceOrderCommand` built from plain `int`, `decimal`, and `string` values, and prints the non-zero `Id` it returns. A reload through `IOrderRepository` in a separate scope then prints that order's `Lines.Count`, proving the handler really persisted it.
- Step 6's caller never names `Order`, `Money`, `IOrderRepository`, or `OrderingContext`. `PlaceOrderHandler` and `PlaceOrderCommand` are the only types it touches.
- No "Not Yet Implemented" box prints anywhere in the output.

## Student Changes

### 1. Delete the "Not Yet Implemented" box
- File: ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstrateRepositoryEndToEnd.cs
- Method: RunEndToEndAsync
- Find TODO marker: `Module 4 Clip 9 — Delete the "Not Yet Implemented" box below.`
- Action: Delete the TODO comment and the `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call directly below it. Both sit at the top of the method, above the commented walkthrough.

### 2. Uncomment the full end-to-end walkthrough
- File: same
- Find TODO marker: `Module 4 Clip 9 — Uncomment the end-to-end walkthrough below.`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it (the seed step, Steps 1 through 6, and the silent cleanup at the end). This is the only content in the method; once uncommented, nothing else follows it.
