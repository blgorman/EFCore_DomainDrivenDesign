# Clip06 - Persisting Aggregates: Add, Remove, and SaveAsync
Shows that staging is not saving. `Add` puts the order in the `ChangeTracker` as `Added` with no SQL issued, `SaveAsync` flushes the order in one transaction and a fresh scope confirms the row reached the database, and `Remove` marks the order `Deleted` before any `DELETE` runs, after which `GetByIdAsync` returns null. Closes on why `SaveAsync` belongs on `IOrderRepository` rather than exposing `SaveChangesAsync` to callers.

## Demo Class
- DemonstratePersistAggregate.ShowWriteSideAsync

## Tests
No unit tests are introduced in this clip.

## Files To Edit
- ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstratePersistAggregate.cs

This clip requires having completed Clip 4 (`OrderRepository`'s `Add`, `Remove`, `SaveAsync`, and `GetByIdAsync` must already be implemented) — without it, the code below won't behave as described.

## What To Verify
- Part 1 prints `EntityState.Added` for the new order immediately after `writeRepo.Add(newOrder, SeedDataHelper.CustomerAId)`, before any `SaveAsync` call.
- Part 2 prints the reloaded order's `Id`, `CustomerId` (via the shadow-property accessor), `Status`, and `Lines.Count` — loaded through `readBackRepo` in a brand-new scope, proving the insert reached the database.
- Part 3 prints `EntityState.Deleted` immediately after `deleteRepo.Remove(toDelete)`, then confirms `confirmDeleteRepo.GetByIdAsync` returns `null` after `SaveAsync`.
- Part 4 prints the `IOrderRepository` interface's method list and explains why `SaveAsync` is the only flush path callers can reach.
- No "Not Yet Implemented" box prints anywhere in the output.

## Student Changes

### 1. Delete the "Not Yet Implemented" box
- File: ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstratePersistAggregate.cs
- Method: ShowWriteSideAsync
- Find TODO marker: `Module 4 Clip 6 — Delete the "Not Yet Implemented" box below.`
- Action: Delete the TODO comment and the `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call directly below it. Both sit at the top of the method, above the commented walkthrough.

### 2. Uncomment the full write-side walkthrough
- File: same
- Find TODO marker: `Module 4 Clip 6 — Uncomment the write-side walkthrough below.`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it (the seed step and Parts 1 through 4). This is the only content in the method; once uncommented, nothing else follows it.
