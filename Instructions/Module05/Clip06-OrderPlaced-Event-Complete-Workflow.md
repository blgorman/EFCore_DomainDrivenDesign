# Clip06 - DEMO: OrderPlaced Event Complete Workflow
Unlock the final two screens and run the full pipeline end to end — `Order.Place()` raises the event, the aggregate holds it, the save commits, the interceptor dispatches, and the handler logs.

Module 5 depends on all previous modules having been successfully implemented.

## Demo Class
- DemonstrateOrderPlacedEvent.RunEndToEndAsync

## Tests

Module 5's two integration tests run here, at the end of the module. Run them before starting
Module 6.

**Integration tests (requires Docker):**
```powershell
dotnet test --filter "Module=5&Category=Integration"
```
Expected: 2 pass, 0 fail.

`DomainEventDispatchTests` places an order through `IOrderRepository`, saves it, and asserts that
`DomainEventDispatchInterceptor` dispatched `OrderPlacedEvent` exactly once and then cleared the
aggregate's event list. Together they are the automated form of what Stages 4 and 5 of this clip's
demo show on screen.

These tests need Module 4 Clip 4 complete: they resolve `IOrderRepository`, call the
`Add(order, customerId)` overload that sets the shadow `CustomerId`, and call `SaveAsync`. All three
are stubs until that clip is implemented, so running them earlier fails.

**Regression — prior modules should still pass:**
```powershell
dotnet test --filter "Module=2"
dotnet test --filter "Module=3"
dotnet test --filter "Module=4"
```
Expected: 40 pass, 1 pass, 2 pass.
## Student Changes

Stage 4 and Stage 5 each have one screen, and each one is gated on its own. There are two `Not Yet Implemented` boxes to delete and two blocks to uncomment.

### 1. Delete the Stage 4 "Not Yet Implemented" box
- File: ConsoleAppProject/CodeAndDemonstrations/Module05/DemonstrateOrderPlacedEvent.cs
- Find TODO marker: the first `Module 5 Clip 6 — Delete this Not Yet Implemented box:`, below the `Stage 4 — Handler Ran: Side Effect Logged` banner.
- Action: Delete the TODO comment and the active `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call below it.

### 2. Uncomment the Stage 4 handler box
- File: same
- Find TODO marker: `Module 5 Clip 6 — Uncomment the Stage 4 handler box below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it, normalizing indentation as you go. The block reads the captured handler line and prints the "Stage 4 — Handler Ran Against a Committed Row" box.

### 3. Delete the Stage 5 "Not Yet Implemented" box
- File: same
- Find TODO marker: the second `Module 5 Clip 6 — Delete this Not Yet Implemented box:`, below the `Stage 5 — Events Cleared from the Aggregate` banner.
- Action: Delete the TODO comment and the active `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call below it. Leave the `WaitForUserInput` separator above the Stage 5 banner in place — it is the wait between the Stage 4 and Stage 5 screens.

### 4. Uncomment the Stage 5 cleared-events box
- File: same
- Find TODO marker: `Module 5 Clip 6 — Uncomment the Stage 5 cleared-events box below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the "Stage 5 — Aggregate Is Clean" box below it, normalizing indentation as you go.
