# Clip07 - Multiple DbContexts in Practice
End-to-end demo: DI registration, independent model snapshots, a real write through OrderingContext, and unlocking the read back through ShippingContext.

## Demo Class
- DemonstrateMultipleContexts.RunEndToEndAsync

## Tests
No unit tests are introduced in this clip.

## Files To Edit
- ConsoleAppProject/CodeAndDemonstrations/Module03/DemonstrateMultipleContexts.cs

## Prerequisite
Clip 4 must be complete (ShippingContext needs its DbSet and configuration registered).

## What To Verify
- Screen 3's `ReferenceEquals` checks all print `False` (separate Model, ChangeTracker, instance).
- Screen 4 writes a real order via `orderingCtx.SaveChangesAsync()`.
- Screen 5 reads the same row back via `shippingCtx.Shipments` and the Ids match.
- Screen 6 ships the order via OrderingContext and ShippingContext sees the new status after `ChangeTracker.Clear()` and a re-query.

## Student Changes

### 1. Unlock Screen 5 in DemonstrateMultipleContexts
- File: ConsoleAppProject/CodeAndDemonstrations/Module03/DemonstrateMultipleContexts.cs
- Find TODO marker: `Module 3 Clip 7 — Step 1: Delete this entire "Not Yet Implemented" box`
- Action: Delete the TODO comment and the entire NTI box that follows it.
- Find TODO marker: `Module 3 Clip 7 — Step 2: Uncomment this block (requires having previously completed Clip 4).`
- Action: Remove the `//` prefix from every line in the Screen 5 block. Delete the TODO comment. A single active `WaitForUserInput` separator already sits after this block, leading into Screen 6's NTI box — it does not need to be added or removed.

### 2. Unlock Screen 6 in DemonstrateMultipleContexts
- File: same
- Find TODO marker: `Module 3 Clip 7 — Step 3: Delete this entire "Not Yet Implemented" box`
- Action: Delete the TODO comment and the entire NTI box that follows it.
- Find TODO marker: `Module 3 Clip 7 — Step 4: Uncomment this block (requires having previously completed Clip 4).`
- Action: Remove the `//` prefix from every line in the Screen 6 block. Delete the TODO comment. A single active `WaitForUserInput` separator already sits after this block, leading into Screen 7 — it does not need to be added or removed.
