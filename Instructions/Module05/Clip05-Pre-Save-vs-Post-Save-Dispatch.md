# Clip05 - Pre-Save vs. Post-Save Dispatch
Unlock the measurement screen — one save, with the event count, the committed row, and the handler's own output all read back and printed together.

Module 5 depends on all previous modules having been successfully implemented.

## Demo Class
- DemonstratePrePostSaveDispatch.CompareDispatchTimingAsync

## Tests
No unit tests are introduced in this clip.

## Files To Edit
- ConsoleAppProject/CodeAndDemonstrations/Module05/DemonstratePrePostSaveDispatch.cs

This clip requires having completed Clips 3 and 4 — without them, the events are never cleared and the handler never runs.

## What To Verify
- The demo is three screens and stops for input twice.
- Screen 2 prints `SavingChangesAsync declared (pre-save): False` and `SavedChangesAsync declared (post-save): True`.
- Screen 3 prints `Events on aggregate before save: 1`, `Events on aggregate after save: 0`, `DB row exists after save: True`, and a real `Saved row Id` — all from one save.
- Screen 3's `Handler output:` line reads `Order placed: 0`. The `0` is correct: `Order.Place()` raises the event inside the factory before any INSERT runs, so the event carries `0` while the row above it carries its real id.
- The handler line comes from a logger the demo builds for itself. No environment variable is required.
- No "Not Yet Implemented" box prints anywhere in the output.
- No "OrderRepository Is Not Implemented Yet" box prints. That box appears only when `OrderRepository` is still a stub.

## Student Changes

### 1. Delete the "Not Yet Implemented" box
- File: ConsoleAppProject/CodeAndDemonstrations/Module05/DemonstratePrePostSaveDispatch.cs
- Find TODO marker: `Module 5 Clip 5 — Delete this Not Yet Implemented box:`
- Action: Delete the TODO comment and the active `Console.Write(OutputHelpers.BoxedArrayWithTitle("Not Yet Implemented", ...));` call below it.

### 2. Uncomment the measurement block
- File: same
- Find TODO marker: `Module 5 Clip 5 — Uncomment the Step 3 measurement block below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line in the block below it, normalizing indentation as you go. The block builds the capturing provider, places and saves one order through it, reads back the event counts and the committed row, prints the "One Save — Measured" box, and removes the order again.
