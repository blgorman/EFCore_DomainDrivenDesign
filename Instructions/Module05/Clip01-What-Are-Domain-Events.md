# Clip01 - What Are Domain Events and Where Do They Live?
Live demo, but no code to write — the starter file is already fully implemented for this clip.

Module 5 depends on all previous modules having been successfully implemented.

## Demo Class
- DemonstrateDomainEvents.WalkInterfacesAsync

## Tests
No unit tests are introduced in this clip.

## What To Verify
- Screen 2 prints reflection results for `IDomainEvent`: zero declared members and zero base interfaces — it is a marker interface.
- Screen 3 prints `OrderPlacedEvent` inspection: `Is record (immutable): True`, `Implements IDomainEvent: True`, and its two properties (`OrderId`, `PlacedAt`) with live instance values. The `OrderId` is a five-digit random value labeled `(<= randomly generated, not a saved order)` — nothing has been saved at this point.
- Screen 4 lists exactly three concrete `IDomainEvent` implementations (`OrderPlacedEvent`, `OrderShippedEvent`, `OrderCancelledEvent`), all in `OrderManagement.Domain.Events`.
- Screen 5 prints the five-stage lifecycle (created → queued → interceptor → handler → cleared).

## Student Changes
None. No code changes for this clip — `DemonstrateDomainEvents.cs` has no `//TODO` gate; every screen already runs. The demo is pure reflection and never touches the database.
