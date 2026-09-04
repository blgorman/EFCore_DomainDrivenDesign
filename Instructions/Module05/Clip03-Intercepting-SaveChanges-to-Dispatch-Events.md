# Clip03 - Intercepting SaveChanges to Dispatch Events
Implement `DomainEventDispatchInterceptor.SavedChangesAsync` so every save on `OrderingContext` collects, dispatches, and clears queued domain events, then unlock the demo's verification screen.

Module 5 depends on all previous modules having been successfully implemented.

## Demo Class
- DemonstrateSaveChangesInterceptor.ShowInterceptorAsync

## Tests
No unit tests are introduced in this clip.

## Files To Edit
- OrderManagement.Infrastructure/Events/DomainEventDispatchInterceptor.cs
- ConsoleAppProject/CodeAndDemonstrations/Module05/DemonstrateSaveChangesInterceptor.cs

Note: `DomainEventDispatcher.DispatchAsync` is still a stub, so no handler log output appears yet — this clip proves collection, timing, and clearing.

## What To Verify
- Screen 1 prints `SavedChangesAsync` in the interceptor's declared overrides (post-save dispatch) and notes `SavingChangesAsync` is not overridden.
- Screen 4 prints `Events remaining on aggregate: 0` (the interceptor called `ClearEvents()`) and `DB row found via FindAsync: True` — both from a single save.
- No "OrderRepository Is Not Implemented Yet" box prints. That box appears only when `OrderRepository` is still a stub, and the demo returns without reaching Screen 4.
- No handler log line appears during the save — expected, because the dispatcher is still a stub.
- The solution builds with zero compiler errors.

## Student Changes

### 1. Implement SavedChangesAsync
- File: OrderManagement.Infrastructure/Events/DomainEventDispatchInterceptor.cs
- Find TODO marker: `Module 5 Clip 3 — Uncomment the implementation below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it (the null guard, the `holders` scan, the `events` snapshot, and the `try`/`finally` dispatch-and-clear), normalizing indentation as you go. Leave the active `return await base.SavedChangesAsync(eventData, result, ct);` at the bottom of the method exactly where it is — the uncommented block runs above it.

### 2. Delete the "Not Yet Implemented" box
- File: ConsoleAppProject/CodeAndDemonstrations/Module05/DemonstrateSaveChangesInterceptor.cs
- Find TODO marker: `Module 5 Clip 3 — Delete this Not Yet Implemented box:`
- Action: Delete the TODO comment and the active `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call below it.

### 3. Uncomment the verified-results box
- File: same
- Find TODO marker: `Module 5 Clip 3 — Uncomment the verified-results box below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the "Interceptor Has Fired — Verified Results" box below it, normalizing indentation as you go.
