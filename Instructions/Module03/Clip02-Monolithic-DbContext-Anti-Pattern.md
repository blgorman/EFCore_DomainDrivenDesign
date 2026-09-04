# Clip02 - The Monolithic DbContext Anti-Pattern
Live demo, but no code to write — the starter file is already fully implemented for this clip.

## Demo Class
- DemonstrateMonolithicContext.ShowProblemsAsync

## Tests
No unit tests are introduced in this clip.

## What To Verify
- The Ordering-team and Shipping-team "Order" boxes show different property lists.
- `MonolithicContext.Model.GetEntityTypes()` prints 13 registered entity types.
- The three problem screens (God Class, Mixed Concerns, Shared Migrations) render after the entity-type list.

## Student Changes
None. No code changes for this clip — `DemonstrateMonolithicContext.cs` has no `//TODO` gate; every screen already runs.
