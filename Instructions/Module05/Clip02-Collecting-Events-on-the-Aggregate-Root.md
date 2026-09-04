# Clip02 - Collecting and Managing Events on the Aggregate Root
Live demo, but no code to write — the starter file is already fully implemented for this clip.

Module 5 depends on all previous modules having been successfully implemented.

## Demo Class
- DemonstrateDomainEventCollection.ShowEventCollectionAsync

## Tests
No unit tests are introduced in this clip.

## What To Verify
- Screen 1 prints `IDomainEventHolder`'s two members (`DomainEvents`, `ClearEvents`), confirms `AggregateRoot` implements the interface, and finds the private `_domainEvents` field of type `List<IDomainEvent>` via reflection.
- Screen 2 prints an order created through `Order.Place()` with `Order Id: 0` (never saved) and `Events queued: 1`, event type `OrderPlacedEvent`.
- Screen 3 prints the event count dropping from `1` to `0` after `order.ClearEvents()`.

## Student Changes
None. No code changes for this clip — `DemonstrateDomainEventCollection.cs` has no `//TODO` gate; every screen already runs. The demo seeds the two demo customers first so `SeedDataHelper.CustomerAId` holds a real database id (customer seeding is silent — `Customer` raises no domain events), then creates an `Order` in memory that is never saved.
