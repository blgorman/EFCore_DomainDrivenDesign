# CLAUDE.md — EFCore + DDD Pluralsight Course

## What This Project Is

This is a **Pluralsight course starter-file package** for a course on EF Core with Domain-Driven Design patterns. Brian Gorman/Claude is the instructor/author.

The repo ships to students **with code intentionally incomplete**. `//TODO: Module N Clip M` comments are student exercise markers — they tell students exactly what to change at each clip. **Never implement a TODO unless Brian explicitly says "implement Clip N."** Removing a TODO removes the student's exercise.

The app is a console app with an interactive menu system. Every run drops and recreates the database from migrations (`EnsureDeletedAsync` + `MigrateAsync` in `Program.cs`), so the schema is always clean.

---

## Solution Structure

| Project | Role |
|---|---|
| `ConsoleAppProject` | Entry point, menus, and all demo/demonstration files |
| `OrderManagement.Domain` | Domain layer: aggregates, entities, value objects, events, repository interfaces |
| `OrderManagement.Infrastructure` | EF Core contexts, configurations, repositories, specs, migrations, event infrastructure |
| `OrderManagement.Application` | Application handlers (e.g., `PlaceOrderHandler`) |
| `OrderManagement.Api` | ASP.NET Core host (`OrdersController`) showing how a web front end would reach the domain. **Untested illustration — see rule 7** |
| `OrderManagement.Tests` | Unit tests plus Testcontainers-backed integration tests (real SQL Server in Docker) |
| `ConsoleHelpers` | `OutputHelpers` (BoxedArrayWithTitle, SectionBanner, WriteColored) and `InputHelpers` (WaitForUserInput) |
| `AppModels` | Anemic model used only in Module 1 |
| `EFDataLayer` | Simple EF context used only in Module 1 |

---

## Key Domain Types

**`OrderManagement.Domain/Aggregates/Order.cs`**
- `Order` extends `AggregateRoot` (which holds domain events)
- Properties: `Id` (int), `CustomerId` (int — *see shadow property TODO below*), `Status` (OrderStatus), `PlacedAt`, `Total` (decimal — *see Money TODO below*)
- `Lines` is a `List<OrderLine>` in the starter; becomes `private readonly List<OrderLine> _lines` + `IReadOnlyCollection<OrderLine>` at Clip 2
- Factory: `Order.Place(int customerId, IEnumerable<(int productId, int quantity, Money unitPrice)> lines)`
- Business methods: `AddLine`, `Process`, `Confirm`, `Ship`, `Cancel`

**`OrderManagement.Domain/Aggregates/OrderLine.cs`**
- Properties: `Id` (int), `ProductId` (int — **not Guid**), `Quantity` (int), `UnitPrice` (Money), `LineTotal` (Money)
- **No navigation property back to Order** — this is a DDD rule (see DDD Boundaries section)
- `OrderId` is a **shadow FK** in EF Core (`WithOne()` not `WithOne(l => l.Order)`)

**`OrderManagement.Domain/Entities/Customer.cs`**
- `Customer.Create(string name, string email, int? id = null)` — sets `Id = id ?? 0`
- **Never pass an explicit Id** to `Create()` in seeding — SQL Server IDENTITY columns reject explicit values unless `IDENTITY_INSERT` is ON

**`OrderManagement.Domain/ValueObjects/Money.cs`**
- `Money.Create(decimal amount, string currency)` — appears at Module 2 Clip 4
- Used as `UnitPrice` and `LineTotal` on `OrderLine`, and `Total` on `Order` (post-Clip-4)

**`OrderManagement.Domain/Enums/OrderStatus.cs`**
- `Placed = 0`, `Processing = 1`, `Confirmed = 2`, `Shipped = 3`, `Cancelled = 4` (verify in file)

---

## Key Infrastructure Types

**`OrderManagement.Infrastructure/Data/OrderingContext.cs`**
- Write-side context; owns `Orders`, `Customers`
- Uses **explicit `ApplyConfiguration` calls** (NOT `ApplyConfigurationsFromAssembly`) — see "Critical Config Rule" below
- Registered with `DomainEventDispatchInterceptor` in `Program.cs`

**`OrderManagement.Infrastructure/Data/ShippingContext.cs`**
- Read-side context; maps `ShipmentReadModel` (read projection of Orders)
- `ShipmentReadModelConfiguration` uses `ExcludeFromMigrations()` so it never creates/alters the Orders table
- In Program.cs, has a `//TODO: Module 3 Clip 8` to add `MigrationsHistoryTable("__EFMigrationsHistory_Shipping")`

**`OrderManagement.Infrastructure/Repositories/OrderRepository.cs`**
- All methods are stubs with `//TODO: Module 4 Clip 4` comments
- `GetByIdAsync` returns `null`, `Add`/`Remove` are no-ops, `SaveAsync` returns `CompletedTask`, `ListAsync` returns empty array
- Students implement these at Clip 4

**`OrderManagement.Infrastructure/Specifications/`**
- `OrdersByCustomerSpecification(int customerId)` — `//TODO: Module 6 Clip 2` adds `Where` using `EF.Property<int>(o, "CustomerId")`, `//TODO: Module 6 Clip 4` adds `Include`/`OrderBy`
- `OrderSearchSpecification(OrderStatus? status, int? minLines)` — `//TODO: Module 6 Clip 6`

---

## Critical Rules

### 1. Starter File TODO Pattern
`//TODO: Module N Clip M — <instruction>` = student exercise. **Never implement without explicit instruction from Brian.**

### 2. DDD Boundary: No Nav Property from OrderLine → Order
`WithOne()` in `OrderLineConfiguration` — never `WithOne(l => l.Order)`. OrderLine does not navigate to its aggregate root.

### 3. OrderingContext Uses Explicit ApplyConfiguration
```csharp
// DO THIS:
modelBuilder.ApplyConfiguration(new OrderConfiguration());
modelBuilder.ApplyConfiguration(new OrderLineConfiguration());
modelBuilder.ApplyConfiguration(new CustomerConfiguration());

// NOT THIS:
modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingContext).Assembly);
```
`ApplyConfigurationsFromAssembly` picks up `ShipmentReadModelConfiguration` and causes a runtime crash (two entity types mapped to the same table with no linking relationship).

### 4. SeedDataHelper — Never Hardcode Customer Ids
`CustomerAId` and `CustomerBId` are `public static int { get; private set; }` — they are resolved from the DB after save, never hardcoded. `SeedCustomersAsync` seeds by email check, calls `Customer.Create(name, email)` with no Id, then reads back the DB-assigned Id. **Hardcoding an explicit Id causes `IDENTITY_INSERT` errors.**

### 5. ProductId Is int, Not Guid
`OrderLine.ProductId` is `int` (e.g., 2001). Do not use `.ToString()[..8]` or any Guid-style slicing — it throws `ArgumentOutOfRangeException` because "2001" has only 4 chars.

### 6. WaitForUserInput Between Every Pair of BoxedArrayWithTitle Calls
Every demo screen that shows two consecutive `BoxedArrayWithTitle` blocks **must** have a `WaitForUserInput` separator between them:
```csharp
//----------------------------------------------------------------//
Console.WriteLine();
InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
Console.WriteLine();
//----------------------------------------------------------------//
```
Missing these causes content to rush past without the student being able to read it.

### 7. OrderManagement.Api Is an Untested Illustration
The API is the external hook — the layer a website would use against the domain — and it belongs in the solution for that reason. It is **not** working code:

- No test touches it, no clip edits it, no clip runs it.
- It sits on the same stubs as everything else. Until Module 4 Clip 4 implements `OrderRepository`, `GetByIdAsync` returns `null`, so `GET`, `/cancel`, and `/ship` all 404; until `PlaceOrderHandler`'s Clip 4 TODO is uncommented, `POST` returns id `0` and saves nothing.
- It uses its own connection string (`DefaultConnection` → `OrderManagementDDD`, a different database from the console app) and never migrates or seeds at startup.
- Its DI registrations are deliberately minimal: `ICustomerRepository` is not registered, and `ShippingContext` maps nothing until Module 3 Clip 4.

Do not fix any of this — do not add registrations, startup migrations, seeding, or tests — unless Brian explicitly asks. Do not describe the API as working or verified anywhere in the docs. It is a valid EF Core design-time host, so `--startup-project OrderManagement.Api` is legitimate for migration commands, and its `ShippingContext` registration already shows the finished Module 3 Clip 8 `MigrationsHistoryTable` form.

---

## Demo File Pattern

All demo static classes follow this pattern:

```csharp
public static class DemonstrateXxx
{
    public static async Task ShowSomethingAsync(IServiceProvider serviceProvider)
    {
        // Screen 1
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Title"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle("subtitle", new[] { "line1", "line2" }));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // Screen 2 ...
        await Task.CompletedTask;
    }
}
```

### "Not Yet Implemented" (NTI) Box Pattern
When a demo has an entire section commented under a `//TODO: Module N Clip M` block, an NTI box must appear so students know it is intentionally incomplete — not a runtime error:

```csharp
OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Section Title"), ConsoleColor.DarkBlue);
Console.Write(OutputHelpers.BoxedArrayWithTitle(
    "Not Yet Implemented",
    new[]
    {
        "This demo will work after completing Module N Clip M.",
        "Open DemonstrateXxx.cs and uncomment the //TODO: Module N Clip M block.",
        "Prerequisite: <what the student needs to implement first>."
    }
));
//----------------------------------------------------------------//
Console.WriteLine();
InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
Console.WriteLine();
//----------------------------------------------------------------//
```

---

## Module Breakdown

### Module 1 (stable, do not touch)
- Clip 2: `DemonstrateAnemicModel` — uses `AppModels.Order` (entirely separate from domain model)
- Clip 7: Full program demo (just needs everything to run)

### Module 2 (stable, do not touch)
Progressive clip-by-clip DDD hardening of `Order`:

| Clip | Change |
|---|---|
| 2 | `_lines` backing field + `IReadOnlyCollection<OrderLine>` + `HasField` EF config |
| 3 | Private setters on all Order properties; state-transition guards |
| 4 | `Total` changes from `decimal` to `Money`; `OwnsOne` in `OrderConfiguration` |
| 5 | `CustomerId` becomes shadow property (`private int _customerId;`) — CLR property removed |
| 7 | EF materialization proof lines unlocked (private constructor demo) |
| 8 | Guard clauses on `Money.Create`, `OrderLine` constructor, `Order.Place`, `Order.AddLine` |

**Clip 5 Shadow Property Details:**
- Before Clip 5: `public int CustomerId { get; set; }` (or `private set` after Clip 3)
- After Clip 5: `private int _customerId;` — only accessible via:
  - Tracked entity: `ctx.Entry(entity).Property<int>("CustomerId").CurrentValue`
  - LINQ: `EF.Property<int>(o, "CustomerId")`
  - Untracked entities: cannot read shadow props at all

### Module 3 — Bounded Contexts
Menu items map to these files:
1. `DemonstrateMonolithicContext` — static text, no DB needed
2. `DemonstrateFocusedDbContext` — Screen 4 (ShippingContext) has NTI block (`//TODO: Module 3 Clip 4`)
3. `DemonstrateOneTableTwoModels` — shows same Orders table through both contexts
4. `DemonstrateCrossContextReads` — uses `SeedDataHelper.SeedAsync`; has `WaitForUserInput` between all pairs
5. `DemonstrateMultipleContexts` — end-to-end demo
6. `DemonstrateMigrationStrategy` — static text, no DB needed

**Module 3 key concepts:**
- `OrderingContext` and `ShippingContext` are separate; each has its own migrations folder
- `ShipmentReadModel` in `ShippingContext` maps the Orders table read-only via `ExcludeFromMigrations()`
- Migration folders: `Migrations/Ordering/` and `Migrations/Shipping/`

### Module 4 — Repository Pattern

**Menu option number equals clip number for all nine clips.** Keep it that way — every clip has a student instruction doc, so every clip gets a menu entry.

1. `DemonstrateOrderRepositoryInterface` is wired at option 2 and runs entirely in memory (no `OrderingContext`, no `SaveChanges`).
2. `DemonstrateRepositoryTesting` at option 8 is a reference screen only. Clip 8 itself is recorded from `dotnet test --filter Module=4`.

| Option | Clip | Demo class |
|---|---|---|
| 1 | 1 | `DemonstrateGenericRepoAntiPattern` |
| 2 | 2 | `DemonstrateOrderRepositoryInterface` |
| 3 | 3 | `DemonstrateRepositoryInterfaceInDomain` |
| 4 | 4 | `DemonstrateOrderRepository` |
| 5 | 5 | `DemonstrateAggregateLoading` — `ProductId` is int, not Guid. Final screen is an **NTI box**, gated on `//TODO: Module 4 Clip 5` |
| 6 | 6 | `DemonstratePersistAggregate` — **NTI box**, gated on `//TODO: Module 4 Clip 6` |
| 7 | 7 | `DemonstrateUnitOfWork` — **NTI box**, gated on `//TODO: Module 4 Clip 7` |
| 8 | 8 | `DemonstrateRepositoryTesting` |
| 9 | 9 | `DemonstrateRepositoryEndToEnd` — **NTI box**, gated on `//TODO: Module 4 Clip 9` |

Options 6, 7, and 9 unlock once students implement `OrderRepository` in Clip 4. The NTI boxes explain this.

Option 5's gate is independent of `OrderRepository`. Its first four screens always run; the fifth prints EF Core's generated SQL through a local `CreateLoggingContext` helper and unlocks at Clip 5 itself.

**Nothing on a demo screen may name a navigation, entity, or column the model does not have.** `OrderLine.Product` and `Order.Notes` have never existed in this solution. Both were printed as example text in `DemonstrateAggregateLoading.cs` until 2026-07-27. The last remaining instance was removed from `DemonstrateSpecificationWithIncludes.cs` on 2026-08-14. `ThenInclude` is never executed anywhere in this solution.

**`Order.Place` does not set `CustomerId`.** `CustomerId` is a pure shadow property with no CLR member on `Order` (Module 2 Clip 5). Any Module 4 code that creates an `Order` and saves it must set the shadow FK first, or `SaveChangesAsync` throws a foreign key violation:
```csharp
ctx.Entry(order).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
```

### Module 5 — Domain Events
Menu items map to these files:
1. `DemonstrateDomainEvents` — static walkthrough of interfaces
2. `DemonstrateDomainEventCollection` — shows `AggregateRoot` event list
3. `DemonstrateSaveChangesInterceptor` — uses `DomainEventDispatchInterceptor`
4. `DemonstrateEventDispatcher` — shows `DomainEventDispatcher`
5. `DemonstratePrePostSaveDispatch` — compares pre/post-save dispatch timing
6. `DemonstrateOrderPlacedEvent` — end-to-end workflow

All Module 5 demos have full visible content (no NTI boxes needed).

**Domain event infrastructure:**
- `AggregateRoot` holds `List<IDomainEvent>` domain events
- `DomainEventDispatchInterceptor` fires on `SaveChangesAsync` via EF interceptor
- `DomainEventDispatcher` dispatches to registered `IDomainEventHandler<T>` instances
- Handlers: `OrderPlacedEventHandler`, `OrderShippedEventHandler`, `OrderCancelledEventHandler`

### Module 6 — Specification Pattern
Menu items map to these files:
1. `DemonstrateQueryProliferation`
2. `DemonstrateSpecificationBase`
3. `DemonstrateSpecificationWithIncludes`
4. `DemonstrateSpecificationEvaluator`
5. `DemonstrateDynamicSpecComposition`
6. `DemonstrateOrderSearch` — end-to-end

**Specification classes:**
- `OrdersByCustomerSpecification(int customerId)` — `//TODO: Clip 2` adds `Where` using `EF.Property<int>(o, "CustomerId") == customerId`, `//TODO: Clip 4` adds `Include`/`OrderBy`
- `OrderSearchSpecification(OrderStatus? status, int? minLines)` — `//TODO: Clip 6` adds the conditional filters

Uses `Ardalis.Specification` NuGet package. `IOrderRepository.ListAsync(ISpecification<Order>)` applies specs via `SpecificationEvaluator.Default.GetQuery(...)`.

### Module 7 — Architecture Review and Next Steps
Slides-first recap module with two working demos, hidden until explicitly enabled:

- **Menu gate**: the Module 7 entry ships commented out in `MainMenu.cs` under two `//TODO: Module 7` markers (menu line + `case 7`). Enabling is documented in `Instructions/Module07/Clip04-Enable-The-CQRS-And-Outbox-Demos.md`. The gate exists so every earlier module's recorded menu footage stays accurate — **never enable it by default, never add a visible Module 7 entry to the starter menu.**
- **CQRS demo**: `DemonstrateCqrsReadModel` + `IOrderReadRepository` (Application/Contracts) + `OrderReadRepository` (Infrastructure/ReadModels). Read path projects to the existing `OrderSummaryDto` with `AsNoTracking` and a `Select` that reads `CustomerId` via `EF.Property<int>`. `Status` converts to string client-side (the server projection keeps the enum).
- **Outbox demo**: `DemonstrateOutboxPattern` + `OutboxMessage`/`OutboxMessageConfiguration`, `OutboxWriteInterceptor` (pre-save, writes event rows into the same save), and `OutboxOrderingContext` (derives from `OrderingContext`, adds only the outbox entity).
- **No outbox migration, ever**: the `OutboxMessages` table is created at runtime by the demo through raw SQL and vanishes on the next run's database rebuild. Adding it to `Migrations/Ordering/` or adding a `DbSet` to `OrderingContext` would leak the table into every earlier module's schema and invalidate recordings.
- The outbox payload for `OrderPlacedEvent` shows `"OrderId":0` — `Order.Place()` constructs the event before the identity value is assigned, identical to what the Module 5 handlers log. Established behavior, not a bug.

---

## SeedDataHelper

Located at `ConsoleAppProject/CodeAndDemonstrations/SeedDataHelper.cs`.

```csharp
public static class SeedDataHelper
{
    public static int CustomerAId { get; private set; }  // Set from DB after save
    public static int CustomerBId { get; private set; }  // Set from DB after save
    public static readonly int Product1Id = 2001;
    public static readonly int Product2Id = 2002;
    public static readonly int Product3Id = 2003;

    public static async Task SeedAsync(OrderingContext context) { ... }
    public static async Task<Order> EnsureOrderWithLinesAsync(OrderingContext ctx) { ... }
}
```

- `SeedAsync` seeds by email check, never by explicit Id — avoids `IDENTITY_INSERT` errors
- `EnsureOrderWithLinesAsync` is used by Module 4 demos that need a real order before `OrderRepository` is fully implemented
- Product Ids are hardcoded (2001-2003) — these are used to create `OrderLine` records; they are not FK-constrained

---

## Tests

**`OrderManagement.Tests/Integration/OrderRepositoryTests.cs`**
- Integration tests hit a real SQL Server (not mocked)
- Uses `OrdersByCustomerSpecification(int customerId)` to verify customer isolation
- `ListAsync_WithOrdersByCustomerSpec_ReturnsOnlyThatCustomersOrders` verifies only the specified customer's orders are returned

---

## Program.cs Bootstrap

```
Program.Main
├── Host.CreateDefaultBuilder
│   ├── DomainEventDispatcher (Scoped)
│   ├── DomainEventDispatchInterceptor (Scoped)
│   ├── IDomainEventHandler registrations (Scoped)
│   ├── OrderingContext (Scoped, with interceptor)
│   ├── ShippingContext (Scoped, //TODO: Clip 7 adds MigrationsHistoryTable)
│   ├── IOrderRepository → OrderRepository (Scoped)
│   ├── ICustomerRepository → CustomerRepository (Scoped)
│   ├── PlaceOrderHandler (Scoped)
│   └── Application (Transient)
├── EnsureDeletedAsync + MigrateAsync (OrderingContext + ShippingContext)
└── Application.DoWork() → MainMenu → sub-menus → demo static methods
```

Database drops and recreates on **every run** — all demos start from a clean schema.

---

## Migrations

```
OrderManagement.Infrastructure/Migrations/
├── Ordering/
│   ├── 20260402152300_Initial.cs
│   ├── 20260522021806_AddTotalToOrder.cs
│   └── OrderingContextModelSnapshot.cs
└── Shipping/
    ├── 20260402152321_Initial.cs  (intentionally empty — ExcludeFromMigrations)
    └── ShippingContextModelSnapshot.cs
```

Migration commands always require `--context`, `--output-dir`, `--project`, `--startup-project`.

---

## Known Fixed Bugs (do not reintroduce)

1. **IDENTITY_INSERT error**: Caused by passing explicit `Id` to `Customer.Create()` in seeding. Fixed in `SeedDataHelper` — always seed by email check, never pass explicit Id.

2. **`ArgumentOutOfRangeException` on `[..8]` slice**: `ProductId` is `int`, not Guid. `"2001"[..8]` throws. Use `l.ProductId` directly, no slice.

3. **Missing `WaitForUserInput` between consecutive `BoxedArrayWithTitle` calls**: Causes content to rush past. Fixed in `DemonstrateCrossContextReads.cs`, `DemonstrateMigrationStrategy.cs`. Always add the separator pattern between every pair.

4. **`OpenOrdersForCustomerSpecification` deleted and replaced**: The named customer spec is now `OrdersByCustomerSpecification(int customerId)` using `EF.Property<int>(o, "CustomerId")` for the shadow FK. Do not reference `OpenOrdersSpecification` or `OpenOrdersForCustomerSpecification` — both are gone.

---

## Known Open Items (resolve when authoring the relevant module)

None currently open.

---

## Skills

- **`code-walkthrough`** (`.claude/commands/code-walkthrough.md`) — applies one clip's student instructions to the repository exactly as a student would. Invoked as `/code-walkthrough Module NN Clip NN`, or by a student asking for the skill by name. It creates a `walkthrough-moduleNN-clipNN` branch, reads only that clip's `Instructions/ModuleNN/ClipNN-*.md` document, applies every step verbatim, **stops at the first error without fixing it** (reporting the error, the likely cause, and what a fix would take), and **never commits**, so the student reads the diff.
- This is the only skill in the student repository. Narration, slide-plan, and script-cleanup authoring happens in the separate `EFCore_DDD` repository and deliberately has no counterpart here.

---

## Style and Conventions

- No summary comments at top of methods
- No XML doc comments
- Console output uses `OutputHelpers.BoxedArrayWithTitle` and `OutputHelpers.SectionBanner`
- Section banners are `ConsoleColor.DarkBlue`; wait prompts are `ConsoleColor.DarkYellow`
- Every demo ends with `await Task.CompletedTask` (methods are `async Task` even when they have no actual awaits in the starter)
