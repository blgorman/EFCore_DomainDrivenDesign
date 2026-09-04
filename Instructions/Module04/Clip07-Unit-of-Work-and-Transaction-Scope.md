# Clip07 - Unit of Work and Transaction Scope
Shows that `OrderingContext` already is the Unit of Work. Two `Add` calls in one scope land in one `ChangeTracker` with no SQL issued, a single `SaveAsync` commits both orders in one transaction, and a fresh scope confirms both rows. Then two more orders are staged and an exception fires before `SaveAsync` is reached, and a new scope confirms neither order exists in the database.

## Demo Class
- DemonstrateUnitOfWork.ShowUoWBehaviorAsync

## Tests
No unit tests are introduced in this clip.

## Files To Edit
- ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstrateUnitOfWork.cs

This clip requires having completed Clip 4 (`OrderRepository`'s `Add`, `SaveAsync`, `GetByIdAsync`, and `Remove` must already be implemented) — without it, the code below won't behave as described.

## What To Verify
- Part 1 prints `EntityState.Added` for both `order1` and `order2` after two `stagingRepo.Add()` calls with no `SaveAsync` yet.
- Part 2 prints both orders reloaded through a fresh scope after a single `stagingRepo.SaveAsync()` — proving one flush committed both.
- Part 3 prints `False` for both `Order 3 found` and `Order 4 found` after an exception is thrown before `SaveAsync` is reached — confirming zero writes when the use-case aborts early.
- No "Not Yet Implemented" box prints anywhere in the output.

## Student Changes

### 1. Delete the "Not Yet Implemented" box
- File: ConsoleAppProject/CodeAndDemonstrations/Module04/DemonstrateUnitOfWork.cs
- Method: ShowUoWBehaviorAsync
- Find TODO marker: `Module 4 Clip 7 — Delete the "Not Yet Implemented" box below.`
- Action: Delete the TODO comment and the `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call directly below it. Both sit at the top of the method, above the commented walkthrough.

### 2. Uncomment the full Unit of Work walkthrough
- File: same
- Find TODO marker: `Module 4 Clip 7 — Uncomment the Unit of Work walkthrough below.`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it (the seed step, Parts 1 through 3, and the silent cleanup at the end). This is the only content in the method; once uncommented, nothing else follows it.
