# EF Core with Domain-Driven Design — Course Starter Files

Starter-file package for the Pluralsight course on applying Domain-Driven Design patterns with Entity Framework Core.

The solution is an interactive .NET 10 console application. Each menu entry runs a demonstration that corresponds to a clip in the course. Alongside the demos is a full DDD-layered `OrderManagement` domain — aggregates, value objects, domain events, repositories, and specifications — mapped to SQL Server with EF Core.

> **This repository ships intentionally incomplete.** Code marked `//TODO: Module N Clip M` is the student exercise for that clip. See [How the exercises work](#how-the-exercises-work).

---

## Prerequisites

| Requirement | Notes |
|---|---|
| .NET 10 SDK | Every project targets `net10.0`. |
| SQL Server LocalDB | Default connection string uses `(localdb)\mssqllocaldb`. Any SQL Server instance works — just change the connection string. |
| Docker Desktop | Required **only** for the `OrderManagement.Tests` integration tests, which start a real SQL Server container via Testcontainers. |
| EF Core CLI tools | `dotnet tool install --global dotnet-ef` — needed for the migration exercises in Modules 2 and 3. |
| An IDE that opens `.slnx` | The solution file is `ConsoleAppProject.slnx` (XML solution format). Recent Visual Studio, Rider, and `dotnet build` all handle it. |

---

## Getting Started

```bash
git clone <this repo>
cd EFCore_DomainDrivenDesign
dotnet build ConsoleAppProject.slnx
dotnet run --project ConsoleAppProject
```

The console app prints a startup banner, then presents the main menu. Pick a module, pick a clip demo, and read the screens. Most demos pause between screens and wait for a keypress.

**The database is dropped and rebuilt from migrations on every run.** `Program.cs` calls `EnsureDeletedAsync()` followed by `MigrateAsync()` before the menu appears, so every demo starts from a clean schema. Do not point the connection string at a database you care about.

### Configuration

`ConsoleAppProject/appsettings.json` holds the connection string:

```json
"ConnectionStrings": {
  "ApplicationDbConnection": "Server=(localdb)\\mssqllocaldb;Database=EFCoreDDD_db;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Configuration is layered: `appsettings.json` → `appsettings.{Environment}.json` → environment variables → user secrets. The console project has a `UserSecretsId`, so `dotnet user-secrets set "ConnectionStrings:ApplicationDbConnection" "..."` overrides the checked-in value without editing the file.

Three environment variables are read before the host is built:

| Variable | Values | Effect |
|---|---|---|
| `DOTNET_ENVIRONMENT` | e.g. `Development` | Selects the `appsettings.{Environment}.json` overlay; shown in the startup banner. |
| `LOG_TO_FILE` | `true` / `false` | Turns Serilog on and writes daily rolling logs to `C:\Logs\`. |
| `LOG_TO_CONSOLE` | `true` / `false` | Adds a Serilog console sink. Only takes effect when `LOG_TO_FILE` is also `true` — Serilog is not configured at all otherwise. |

Domain event handlers log through `ILogger`, so `LOG_TO_FILE=true LOG_TO_CONSOLE=true` is a useful pairing when working through Module 5.

---

## Solution Structure

| Project | Role |
|---|---|
| `ConsoleAppProject` | Entry point, host bootstrap, menu system, and every demonstration class (organized by module folder). |
| `OrderManagement.Domain` | Domain layer: the `Order` aggregate, entities, value objects, enums, domain events, and repository interfaces. No EF Core dependency. |
| `OrderManagement.Infrastructure` | EF Core contexts, entity configurations, repositories, specifications, migrations, domain event infrastructure, outbox, read models. |
| `OrderManagement.Application` | Commands, DTOs, read-side contracts, and `PlaceOrderHandler`. Depends only on Domain. |
| `OrderManagement.Api` | ASP.NET Core host showing how a web front end would reach the same domain. **Architectural illustration only — untested, and not runnable until the course code is implemented.** See [The API Project](#the-api-project). |
| `OrderManagement.Tests` | xUnit unit tests plus Testcontainers-backed integration tests. |
| `ConsoleHelpers` | `OutputHelpers`, `InputHelpers`, `LogCaptureHelpers` — formatted console output and validated input. |
| `AppModels` | The anemic `Order` class used only in Module 1. |
| `EFDataLayer` | A bare `ApplicationDbContext` plus design-time factory, used only in Module 1. |
| `TestConsoleHelpers` | xUnit tests for the `ConsoleHelpers` library. |

`Directory.Build.props` centralizes the target framework and every NuGet package version. Individual `.csproj` files reference versions by MSBuild property (`$(MicrosoftEntityFrameworkCoreVersion)`, and so on) rather than hard-coding numbers.

Key third-party dependencies: **EF Core 10** (SqlServer, Tools, Design), **Ardalis.Specification** and **Ardalis.Specification.EntityFrameworkCore** (Module 6), **Serilog**, and for tests **xUnit**, **Shouldly**, and **Testcontainers.MsSql**.

---

## How the Exercises Work

Each course clip has a matching instruction document under `Instructions/ModuleNN/`, and matching `//TODO: Module N Clip M` markers in the code. The markers take three forms:

1. **Replace this line** — a working starter line sits above a commented target line.
2. **Uncomment this block** — the finished code is present but commented out.
3. **Implement this method** — a stub (`return null`, empty body) with the intent described in the comment.

Demos whose content is entirely gated behind a TODO print a **"Not Yet Implemented"** box instead. That box is expected output for a fresh clone, not an error: it names the module, the clip, and the file to open.

An instruction document lists the demo class it drives, the files to edit, what to verify, and numbered student changes. For example, `Instructions/Module04/Clip04-Implementing-the-Repository-in-Infrastructure.md` walks through turning `OrderRepository`'s six stub methods into working code, with a failing test at the start and a passing one at the end.

`Instructions/` covers Modules 2 through 7. Module 1 is conceptual and has no code exercise beyond running the anemic model demo.

---

## Menu Map

```
Main Menu
├── Module 1: Introduction and DDD Concepts
│     └── Show Anemic Model
├── Module 2: Enforcing Domain Invariants
│     ├── Anemic vs Domain Model Contrast
│     ├── Backing Field Mapping
│     ├── Private Setters vs Private Fields
│     ├── Owned Entities (Money Value Object)
│     ├── Shadow Properties — Hiding the FK
│     ├── Fluent IEntityTypeConfiguration Walkthrough
│     ├── EF Core Materialization
│     ├── Factory Methods and Guard Clauses
│     └── DEMO: Order Aggregate End to End
├── Module 3: Bounded Contexts
│     ├── Monolithic DbContext Anti-Pattern
│     ├── Focused DbContext (Ordering & Shipping)
│     ├── One Table, Two Models
│     ├── Cross-Context Reads
│     ├── DEMO: Multiple DbContexts
│     └── Migration Strategy Terminal Demo
├── Module 4: Repository Pattern            (menu option number == clip number)
│     ├── Generic Repo Anti-Pattern
│     ├── Only Aggregate Roots Get Repositories
│     ├── Repository Interface in Domain Layer
│     ├── OrderRepository Implementation
│     ├── Aggregate Loading (Include Strategy)
│     ├── Persisting Aggregates (Write Side)
│     ├── Unit of Work / Transaction Scope
│     ├── Testing Repositories with Testcontainers
│     └── DEMO: Complete Order Repository
├── Module 5: Domain Events
│     ├── Domain Events Overview (IDomainEvent Walkthrough)
│     ├── Events on Aggregate Root
│     ├── SaveChanges Interceptor
│     ├── Event Dispatcher and Handler
│     ├── Pre-Save vs Post-Save Dispatch
│     └── DEMO: OrderPlaced Complete Workflow
├── Module 6: Specification Pattern
│     ├── Query Proliferation Problem
│     ├── Base Specification Class
│     ├── Spec with Include and OrderBy
│     ├── SpecificationEvaluator in Repository
│     ├── Dynamic Specification Composition
│     └── DEMO: Dynamic Order Search End to End
└── Module 7: CQRS and Outbox Next Steps    (hidden by default — see below)
      ├── DEMO: Transactional Outbox
      └── DEMO: CQRS Read Model Beside the Write Model
```

Demo classes live in `ConsoleAppProject/CodeAndDemonstrations/ModuleNN/`, one file per menu entry.

### Module 7 is hidden on purpose

`Module7NextStepsMenu` exists and both of its demos are fully implemented, but the main menu entry ships commented out behind two `//TODO: Module 7` markers in `MainMenu.cs` — one for the menu label, one for the `case 7`. Uncomment both to enable it. `Instructions/Module07/Clip04-Enable-The-CQRS-And-Outbox-Demos.md` documents the step.

The outbox demo creates its `OutboxMessages` table at runtime with raw SQL. There is deliberately no migration and no `DbSet` on `OrderingContext` for it, so the table never appears in the schema any other module sees.

---

## The Domain Model

`OrderManagement.Domain` is the reference model the whole course builds toward. **As it ships**, several pieces are deliberately in their "before" state — Modules 2 and 4 are what harden them.

**`Order` (aggregate root, `Aggregates/Order.cs`)**
- Extends `AggregateRoot`, which holds the `IDomainEvent` collection.
- Ships with public setters, a public `List<OrderLine> Lines`, a CLR `CustomerId`, and a `decimal Total`. Module 2 converts these to private setters, a read-only collection over a `_lines` backing field, a shadow `CustomerId`, and a `Money` total.
- Factory: `Order.Place(int customerId, IEnumerable<(int productId, int quantity, Money unitPrice)> lines)`.
- Behavior: `AddLine`, `Process`, `Confirm`, `Ship`, `Cancel`. State-transition guards are added in Module 2 Clip 8.
- A private parameterless constructor exists for EF Core materialization.

**`OrderLine` (`Aggregates/OrderLine.cs`)**
- `ProductId` is an `int`, not a Guid. `Quantity` is an `int`; `UnitPrice` and `LineTotal` are `Money`.
- **No navigation property back to `Order`.** `OrderId` is a shadow foreign key configured with `WithOne()` — an aggregate member does not navigate to its root.

**`Money` (`ValueObjects/Money.cs`)** — `Money.Create(decimal amount, string currency)`. Mapped as an owned type.

**`Customer` (`Entities/Customer.cs`)** — `Customer.Create(name, email, id = null)`. Seeding never passes an explicit `Id`; SQL Server IDENTITY columns reject that without `IDENTITY_INSERT`.

**`OrderStatus` (`Enums/OrderStatus.cs`)** — `Placed`, `Processing`, `Confirmed`, `Shipped`, `Cancelled`. Persisted as a string via `HasConversion<string>()`.

**Domain events (`Events/`)** — `OrderPlacedEvent`, `OrderShippedEvent`, `OrderCancelledEvent`, raised by the aggregate and collected on `AggregateRoot`.

**Repository interfaces (`Repositories/`)** — `IOrderRepository`, `ICustomerRepository`. Interfaces live in Domain; implementations live in Infrastructure.

---

## Infrastructure

### Two DbContexts

`OrderingContext` is the write side. It owns `Orders` and `Customers` and registers its configurations explicitly:

```csharp
modelBuilder.ApplyConfiguration(new OrderConfiguration());
modelBuilder.ApplyConfiguration(new OrderLineConfiguration());
modelBuilder.ApplyConfiguration(new CustomerConfiguration());
```

Explicit registration is deliberate. `ApplyConfigurationsFromAssembly` would also pick up `ShipmentReadModelConfiguration` and map two entity types to the `Orders` table with no relationship between them, which fails at runtime.

`ShippingContext` is the read side. Module 3 Clip 4 adds its `DbSet<ShipmentReadModel>` and configuration; Clip 5 switches `ShipmentReadModelConfiguration` to `ExcludeFromMigrations()` so the read model never creates or alters the `Orders` table. Both contexts point at the same database — the point of the module is that one table can carry two different models.

### Repositories and specifications

`OrderRepository` implements `IOrderRepository` and is the Module 4 Clip 4 exercise — every method ships as a stub. `ListAsync(ISpecification<Order>)` applies Ardalis specifications through `SpecificationEvaluator.Default.GetQuery(...)`.

Specifications in `OrderManagement.Infrastructure/Specifications/`:

| Specification | Introduced |
|---|---|
| `OrdersByCustomerSpecification(int customerId)` | Module 6 Clip 2 (`Where` on the shadow FK via `EF.Property<int>`), Clip 4 (`Include` + `OrderBy`) |
| `OrdersByStatusSpecification` | Module 6 Clip 5 |
| `LargeOrdersSpecification` | Module 6 Clip 5 (needs Module 2 Clip 4's `Money` total) |
| `MultiLineOrdersSpecification` | Complete in the starter — used as a composition example |
| `OrderSearchSpecification(OrderStatus?, int? minLines)` | Module 6 Clip 6 (conditional composition) |

### Domain event dispatch

`DomainEventDispatchInterceptor` is an EF Core `SaveChanges` interceptor registered on `OrderingContext`. It collects events off tracked aggregate roots and hands them to `DomainEventDispatcher`, which resolves `IDomainEventHandler<T>` implementations from DI. Handlers ship as `OrderPlacedEventHandler`, `OrderShippedEventHandler`, and `OrderCancelledEventHandler`. The interceptor and dispatcher bodies are the Module 5 Clip 3 and Clip 4 exercises.

### Seed data

`ConsoleAppProject/CodeAndDemonstrations/SeedDataHelper.cs` provides shared demo data.

- `SeedAsync(OrderingContext)` seeds customers by email check and then reads back the database-assigned identity values into `CustomerAId` / `CustomerBId`. Those are `public static int { get; private set; }` — never hard-code them.
- `EnsureOrderWithLinesAsync(OrderingContext)` produces a real order with lines, used by Module 4 demos that need data before `OrderRepository` is implemented.
- `Product1Id` through `Product3Id` are the constants `2001`–`2003`. There is no Products table; these are plain values on order lines.

---

## Migrations

```
OrderManagement.Infrastructure/Migrations/
├── Ordering/
│   ├── 20260402152300_Initial.cs
│   ├── 20260522021806_AddTotalToOrder.cs
│   └── OrderingContextModelSnapshot.cs
└── Shipping/
    ├── 20260402152321_Initial.cs   (empty by design — the read model is excluded from migrations)
    └── ShippingContextModelSnapshot.cs
```

Because there are two contexts in one assembly, every EF command needs `--context`, `--project`, `--startup-project`, and — when adding — `--output-dir`:

```bash
dotnet ef migrations add <Name> \
  --project OrderManagement.Infrastructure \
  --startup-project ConsoleAppProject \
  --context OrderingContext \
  --output-dir Migrations/Ordering

dotnet ef database update \
  --project OrderManagement.Infrastructure \
  --startup-project ConsoleAppProject \
  --context OrderingContext
```

Always generate migrations with the CLI. Hand-editing a migration, a `.Designer.cs`, or a model snapshot lets the snapshot drift from what EF would produce, which breaks the next `migrations add`.

Module 3 Clip 8 adds `MigrationsHistoryTable("__EFMigrationsHistory_Shipping")` to the `ShippingContext` registration so each context tracks its own migration history. `OrderManagement.Api/Program.cs` already shows the finished form of that registration.

---

## Tests

### `OrderManagement.Tests`

```bash
dotnet test OrderManagement.Tests
```

Two categories, distinguished by an xUnit `Category` trait:

- **Unit** (`Domain/OrderTests.cs`, `Domain/MoneyTests.cs`, `Application/SpecificationTests.cs`) — no database, no Docker.
- **Integration** (`Integration/`) — `MsSqlFixture` starts a SQL Server container through Testcontainers and applies migrations to it, so **Docker must be running**. On ARM machines the fixture substitutes the Azure SQL Edge image, since the official SQL Server image has no ARM build.

Tests also carry `Module` and `Clip` traits, so you can scope a run to the clip you are working on:

```bash
dotnet test --filter Category=Unit
dotnet test --filter Module=4
dotnet test --filter "Module=6&Clip=6"
```

Several tests ship commented out under a `//TODO: Module 6 Clip 7` marker — they are part of that clip's exercise.

Some tests are expected to fail against a fresh clone. `Add_ThenGetById_ReturnsOrderWithLines` fails until `OrderRepository` is implemented in Module 4 Clip 4; watching it go from red to green is the point of that clip.

### `TestConsoleHelpers`

```bash
dotnet test TestConsoleHelpers
```

xUnit + Shouldly coverage of the helper library. `TestInputHelpers` injects simulated console input with `Console.SetIn` to exercise the retry and confirm loops; `TestOutputHelpers` checks border characters, line lengths, and separator placement.

---

## Console App Plumbing

### Host bootstrap (`Program.cs`)

```
Host.CreateDefaultBuilder
├── Serilog (opt-in via LOG_TO_FILE / LOG_TO_CONSOLE)
├── Configuration: appsettings.json → appsettings.{Env}.json → env vars → user secrets
├── DomainEventDispatcher, DomainEventDispatchInterceptor          (Scoped)
├── IDomainEventHandler<OrderPlaced|OrderShipped|OrderCancelled>   (Scoped)
├── OrderingContext (SQL Server + dispatch interceptor)            (Scoped)
├── ShippingContext (SQL Server)                                   (Scoped)
├── IOrderRepository → OrderRepository                             (Scoped)
├── ICustomerRepository → CustomerRepository                       (Scoped)
├── PlaceOrderHandler                                              (Scoped)
└── Application                                                    (Transient)

then: EnsureDeletedAsync + MigrateAsync (both contexts) → Application.DoWork() → MainMenu
```

### Menu system

```
IAsyncDemo (interface)
    └── BaseMenu (abstract)
            ├── MainMenu
            └── Module1IntroMenu … Module7NextStepsMenu
```

`BaseMenu.ShowAsync` owns the loop — clear the screen, render the menu through `MenuGenerator`, read a validated choice with `InputHelpers.GetInputAsInt(min: 1, max: MenuOptions().Count, confirm: true)`, dispatch to `HandleMenuChoiceAsync`, and repeat until that returns `false`. It is intentionally **not** virtual: a subclass supplies options and handling, never a different loop. Exceptions thrown by a demo are caught and printed so one broken demo cannot take down the menu.

To add a menu, extend `BaseMenu`, override `MenuOptions()` and `HandleMenuChoiceAsync(int)`, and add a `case` in the parent menu that calls `ShowAsync` on it.

`Application.LINE_LENGTH` (`40`) is the width used for menu rendering.

### `ConsoleHelpers`

`OutputHelpers` — all methods return a formatted `string` rather than writing to the console, except `WriteColored`.

| Method | Description |
|---|---|
| `BoxedMessage(message, borderChar, lineLength = 80)` | One message in a box. `*` borders use `*` side indicators; any other border character uses `\|`. |
| `BoxedMessageWithTitle(title, message, lineLength = 80)` | Title box followed by a dashed inner box holding the message. |
| `BoxedArrayWithTitle(title, items[], lineLength = 80)` | Title box followed by each array item on its own row, dash-separated. The workhorse of every demo screen. |
| `BoxedList(items, borderChar, lineLength = 80)` | A `List<string>` inside a bordered box, one item per line. |
| `BoxedListWithTitle(title, items, lineLength = 80)` | Title box followed by each list item, dash-separated. |
| `SectionBanner(label, lineLength = 80)` | An `=`-ruled banner. Demos print these in `ConsoleColor.DarkBlue`. |
| `WriteColored(text, foreground)` | Writes text in a color and restores the previous one. |

`InputHelpers` — every reading method loops until the input is valid, printing guidance on each failed attempt. All four accept an optional `confirm`; when `true` the user is shown their value and asked to confirm before it is accepted.

| Method | Description |
|---|---|
| `GetInputAsDouble(prompt, min, max, confirm)` | A `double` within `[min, max]`. |
| `GetInputAsInt(prompt, min, max, confirm)` | An `int` within `[min, max]`. |
| `GetInputAsBool(prompt, confirm)` | Y/N, case-insensitive, matching any response starting with `y` or `n`. |
| `GetInputAsString(prompt, confirm, allowEmpty)` | A string; with `allowEmpty: false`, re-prompts on empty or whitespace. |
| `WaitForUserInput()` / `WaitForUserInput(ConsoleColor)` | "Press any key to continue…" plus `Console.ReadKey()`. Demos use the color overload with `ConsoleColor.DarkYellow` between screens. |

`LogCaptureHelpers.CaptureFor(categoryContains, messages)` returns an `ILoggerProvider` that collects log messages from matching categories into a list — how Module 5 demos show handler output inline on screen.

---

## The API Project

`OrderManagement.Api` is the external hook into the domain — the layer a website, SPA, or another service would call. The course is taught through the console app, so the API never appears on camera, but it is the reason the domain, application, and infrastructure layers are split the way they are. `OrdersController` is the proof: it depends on `PlaceOrderHandler` and `IOrderRepository` and nothing else. Swapping a console host for an HTTP host requires no change to the domain.

| Endpoint | Path through the layers |
|---|---|
| `POST /api/orders` | `PlaceOrderHandler.HandleAsync(PlaceOrderCommand)` |
| `GET /api/orders/{id}` | `IOrderRepository.GetByIdAsync` |
| `POST /api/orders/{id}/cancel` | `GetByIdAsync` → `order.Cancel()` → `SaveAsync` |
| `POST /api/orders/{id}/ship` | `GetByIdAsync` → `order.Ship()` → `SaveAsync` |

Cancel and ship show the pattern most clearly: the controller loads the aggregate, calls a behavior method, and saves. It never assigns `Status` itself. The state-transition guards added in Module 2 Clip 8 are what reject an illegal transition, and the controller's only job is turning the resulting `InvalidOperationException` into a `400`.

### ⚠️ Untested, and not runnable as shipped

**Treat this project as an architectural illustration, not a working service.** No test in the solution touches it, no clip exercise edits it, and no clip runs it. Three things must be true before it does anything useful:

**1. The course code has to be implemented first.** The API sits on the same stubs as everything else. Until Module 4 Clip 4 fills in `OrderRepository`, `GetByIdAsync` returns `null`, so `GET`, `/cancel`, and `/ship` all return `404`. Until the `//TODO: Module 4 Clip 4` block in `PlaceOrderHandler` is uncommented, `POST` never saves and returns id `0`. Module 2's aggregate work is a prerequisite for both.

**2. The database has to be created by hand.** The API uses its own connection string — `DefaultConnection` → `OrderManagementDDD`, a *different* database from the console app's `EFCoreDDD_db` — and unlike the console app it never calls `EnsureDeleted`/`Migrate` at startup. Create the schema first:

```bash
dotnet ef database update \
  --project OrderManagement.Infrastructure \
  --startup-project OrderManagement.Api \
  --context OrderingContext
```

There is no seeding either, so a `Customer` row must already exist for any `POST` to satisfy the order's foreign key.

**3. The DI registrations are incomplete.** `Program.cs` registers only what today's controller needs. `ICustomerRepository` is *not* registered, so any new endpoint touching customers fails to resolve at request time. `ShippingContext` is registered but maps nothing until Module 3 Clip 4 adds its `DbSet` and configuration.

`OrdersController.GetOrder` also returns the aggregate straight from the repository. The comment above it notes that a real API would project to a DTO instead; the leak is left in deliberately as a discussion point.

What the API *is* good for as shipped: it is a valid EF Core design-time host, so `--startup-project OrderManagement.Api` works for migration commands. Its `ShippingContext` registration also already shows the finished `MigrationsHistoryTable("__EFMigrationsHistory_Shipping")` form that Module 3 Clip 8 has students write in the console app.

---

## Troubleshooting

| Symptom | Cause and fix |
|---|---|
| A demo prints a "Not Yet Implemented" box | Expected. That clip's exercise is not done yet; the box names the module, clip, and file. |
| `Connection string 'ApplicationDbConnection' not found.` | `appsettings.json` did not reach the output folder, or an override cleared it. Rebuild, or set the value with `dotnet user-secrets`. |
| Integration tests hang or fail to start | Docker is not running. `MsSqlFixture` needs it to start a SQL Server container. |
| `IDENTITY_INSERT` error while seeding | An explicit `Id` was passed to `Customer.Create`. Seed by email check and read the assigned id back from the database. |
| Migration commands fail with "more than one DbContext" | Add `--context OrderingContext` (or `ShippingContext`) along with `--project` and `--startup-project`. |
| `OrderManagement.Api` returns 404 for every order | Expected until Module 4 Clip 4 implements `OrderRepository`. See [The API Project](#the-api-project). |
| API request fails with an invalid object name | The API uses its own database (`OrderManagementDDD`) and never migrates at startup. Run `dotnet ef database update --startup-project OrderManagement.Api --context OrderingContext`. |
| SQL error 56, `Unable to load the SQLUserInstance.dll` | A broken LocalDB client install, not a repo problem. Try `sqllocaldb start mssqllocaldb`; if that does not help, repair the SQL Server Express LocalDB feature or point the connection string at another SQL Server instance. |
| Integration tests fail with `exec format error` | An ARM machine pulled the amd64 SQL Server image. `MsSqlFixture` already substitutes Azure SQL Edge on ARM — make sure you are on current code and that Docker is not forcing a platform. |
| Your work disappeared from the database | By design — every run calls `EnsureDeletedAsync()` before applying migrations. |

---

## License

MIT. See [LICENSE](LICENSE). Copyright © 2026 MajorGuidanceSolutions.
