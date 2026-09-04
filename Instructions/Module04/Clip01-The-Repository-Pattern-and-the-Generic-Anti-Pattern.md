# Clip01 - The Repository Pattern: Intent and the Generic Repository Anti-Pattern
Runs a generic `IRepository<T>` against real seeded orders and shows three failures in live output: `UpdateAsync` marking every column modified on an order nobody edited, `GetAllAsync` returning the whole table with no filter or limit, and `GenericRepository<OrderLine>` compiling and running for a child entity that is not an Aggregate Root. Closes by contrasting each failure against what `IOrderRepository` removes.

## Demo Class
- DemonstrateGenericRepoAntiPattern.ContrastApproachesAsync

## Tests
No unit tests are introduced in this clip.

## Files To Edit
- ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstrateGenericRepoAntiPattern.cs

## What To Verify
The demo runs eight screens once your changes are in. Before your changes it runs seven, and the fourth screen is the "Not Yet Implemented" box.

- Part 3 prints the `Order` entity's `EntityState` and modified-properties list before and after `UpdateAsync` runs. The "after" list shows every mapped property flagged, even though nothing changed.
- Part 3 then prints the full-row `UPDATE` that `SaveChanges` would emit, listing `CustomerId`, `Status`, and `PlacedAt` as unchanged.
- Part 3b prints `Status` at three points: `Placed` when your code loads the order, `Shipped` after the shipping clerk saves, and `Placed` again after your `UpdateAsync` and `SaveChangesAsync`. No exception is raised at any point.
- Part 4 prints every order `GenericRepository<Order>.GetAllAsync()` returned, and in the same box the in-memory filter workaround with a rows-fetched versus rows-needed count.
- Part 5 runs `new GenericRepository<OrderLine>(ctx).GetAllAsync()` directly against `OrderLine`, a child entity, and in the same box lists what else a caller could do without going through `Order`.
- Part 6 prints the closing contrast table (`GenericRepository<T>` column vs `IOrderRepository` column).

## Student Changes

Do these in the order given. The lost-update walkthrough in step 3 calls `ReadStatusAsync`, so uncommenting that walkthrough before the helper leaves the solution with `CS0103: The name 'ReadStatusAsync' does not exist in the current context`. In this order the solution compiles after every step.

### 1. Uncomment the ReadStatusAsync helper
- File: ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstrateGenericRepoAntiPattern.cs
- Find TODO marker: `Module 4 Clip 1 — Uncomment the ReadStatusAsync helper below.`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the method below it. `ReadStatusAsync` opens its own scope on every call, so each reading comes from the database rather than from a `ChangeTracker` that already holds a copy of the order.

### 2. Delete the "Not Yet Implemented" box
- File: same
- Method: ContrastApproachesAsync
- Find TODO marker: `Module 4 Clip 1 — Delete the "Not Yet Implemented" box below.`
- Action: Delete the TODO comment and the `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call directly below it. Leave the section banner line above the TODO in place.

### 3. Uncomment the lost-update walkthrough
- File: same
- Find TODO marker: `Module 4 Clip 1 — Uncomment the lost-update walkthrough below.`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it (the `scopeA` block, through its closing brace). The `WaitForUserInput` separator already sitting below this block stays where it is.
