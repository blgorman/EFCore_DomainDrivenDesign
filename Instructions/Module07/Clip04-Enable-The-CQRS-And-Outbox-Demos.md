# Clip04 - Enable the CQRS and Outbox Demos
Module 7 ships with two working demos that are hidden from the main menu: a CQRS read model beside the write model, and a transactional outbox. Uncomment two lines in `MainMenu.cs` to add the Module 7 entry to the menu, then run both demos. There is no other code to write — the pattern code is complete and this clip is about reading it and watching it run.

## Demo Classes
- DemonstrateCqrsReadModel.ShowReadModelAsync
- DemonstrateOutboxPattern.ShowOutboxAsync

## Tests
No new tests are introduced in this clip.

## Files To Edit
- ConsoleAppProject/Menus/MainMenu.cs

## Files To Review (the pattern code — no edits)
- OrderManagement.Application/Contracts/IOrderReadRepository.cs
- OrderManagement.Infrastructure/ReadModels/OrderReadRepository.cs
- OrderManagement.Infrastructure/Outbox/OutboxMessage.cs
- OrderManagement.Infrastructure/Outbox/OutboxWriteInterceptor.cs
- OrderManagement.Infrastructure/Outbox/OutboxOrderingContext.cs

## Note On The Outbox Table

A production outbox puts `OutboxMessages` in a migration alongside every other table. This course creates the table at runtime instead, and the difference is a compromise worth knowing about before reading the demo code.

`DemonstrateOutboxPattern` issues a raw `CREATE TABLE` statement, and the database rebuild on the next application start drops the table again. `OutboxOrderingContext` holds the `DbSet<OutboxMessage>` so that `OrderingContext` never maps the outbox entity.

The reason is course mechanics rather than design. `OrderingContext` drops and re-migrates the database on every run, so an outbox migration would add `OutboxMessages` to the schema for Modules 1 through 6 as well.

Do not add an outbox migration, and do not add a `DbSet<OutboxMessage>` to `OrderingContext`. Write the migration in an application of your own.

## What To Verify
- The main menu now lists `7] Module 7: CQRS and Outbox Next Steps`, and `Exit` has moved to `8]`. Before this change, the menu matches every earlier module's recording exactly — that is why the entry ships commented out.
- CQRS demo, screen 2: `repository.GetByIdAsync` prints one full aggregate with its `Status`, line count, and `Total`.
- CQRS demo, screen 3: one `OrderSummaryDto` row per seeded order, newest first, with statuses including `Placed`, `Shipped`, and `Cancelled`. No `Order` aggregate is materialized on this path.
- Outbox demo, screen 2: the new order and one `OutboxMessage` row commit in the same `SaveChangesAsync`. The payload shows `"OrderId":0` — `Order.Place()` constructs the `OrderPlacedEvent` before the database assigns the identity value, the same behavior the Module 5 handlers log.
- Outbox demo, screen 3: the relay pass publishes each row, stamps `ProcessedAtUtc`, and reports `Unpublished rows remaining: 0`.
- The solution builds with zero compiler errors.

## Student Changes

### 1. Uncomment the menu entry
- File: ConsoleAppProject/Menus/MainMenu.cs
- Find TODO marker: `Module 7 — Uncomment the menu entry below to enable the Module 7 demos:`
- Action: Delete the TODO comment and remove the `//` prefix from the line below it:
  ```csharp
  "Module 7: CQRS and Outbox Next Steps",
  ```

### 2. Uncomment the menu case
- File: ConsoleAppProject/Menus/MainMenu.cs
- Find TODO marker: `Module 7 — Uncomment the case below to enable the Module 7 demos:`
- Action: Delete the TODO comment and remove the `//` prefix from the line below it:
  ```csharp
  case 7: next = new Module7NextStepsMenu(_serviceProvider); title = "Module 7: CQRS and Outbox Next Steps"; break;
  ```

### 3. Restore the Total line in the CQRS write-path screen
- File: ConsoleAppProject/CodeAndDemonstrations/Module07/DemonstrateCqrsReadModel.cs
- Find TODO marker: `Module 7 Clip 4 — Delete the line below and uncomment the //WITH: lines`
- Action: Delete the TODO comment and the active line below it, then remove the `//` prefix from the two `//WITH:` lines so the screen reads:
  ```csharp
  $"Order {orderAggregate.Id}: Status {orderAggregate.Status}, {orderAggregate.Lines.Count} line(s), " +
  $"Total {orderAggregate.Total.Amount} {orderAggregate.Total.Currency}",
  ```
- The starter ships the shorter line because `Order.Total` is a `decimal` until Module 2 Clip 4 changes the type to `Money`, and the solution has to compile before that change is made.

### 4. Run both demos
- Run the application, choose `7`, and run option `1` (CQRS read model) and option `2` (transactional outbox).
