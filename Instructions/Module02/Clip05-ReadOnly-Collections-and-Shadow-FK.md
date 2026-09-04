# Clip05 - Shadow Properties — Hiding the FK
Make Order.CustomerId a shadow property so the raw FK integer is no longer part of the domain's public surface. Uses OrderLine.OrderId as the already-present reference example.

## Demo Class
- DemonstrateReadOnlyCollections.ShowCollectionAndShadowPropsAsync

## Files To Edit
- OrderManagement.Domain/Aggregates/Order.cs
- OrderManagement.Infrastructure/Configurations/OrderConfiguration.cs
- ConsoleAppProject/CodeAndDemonstrations/SeedDataHelper.cs
- ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateReadOnlyCollections.cs
- ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateAnemicVsDomainModel.cs

## Plumbing Note
- Demo already seeds via SeedDataHelper.EnsureOrderWithLinesAsync.
- Do not switch to SeedDataHelper.SeedAsync in Module 2.
- OrderLine.OrderId is ALREADY configured as a shadow FK from the start of the project.
  This is intentional — it is the "look at me for help" reference that Clip 5 contrasts against.

## What To Verify
- Part 1 of the demo renders without changes — shows OrderLine shadow OrderId via EF Entry API.
- After student changes: Part 2 can be confirmed by running the demo again and verifying
  that order.CustomerId no longer compiles anywhere (it's been removed from Order).
- Uncomment the Part 3 block to prove ctx.Entry(order).Property<int>("CustomerId").CurrentValue works.

## Tests

### Integration test (requires Docker)

```powershell
dotnet test --filter "Module=2&Clip=5&Category=Integration"
```
Expected: 1 pass, 0 fail (before and after this clip)

This test proves the shadow FK read/write pattern works end-to-end via
`ctx.Entry(order).Property<int>("CustomerId").CurrentValue`. It passes both before and after
this clip — before Clip 5 the EF Entry API reads the CLR property through the change tracker;
after Clip 5 it reads the shadow state. There is no compile-time gate because the CLR property
removal is a compile-time change that cannot be expressed as a pre-existing test.

### Regression — prior clips should still pass after this clip:
```powershell
dotnet test --filter "Module=2&Clip=2&Category=Unit"
dotnet test --filter "Module=2&Clip=3&Category=Unit"
dotnet test --filter "Module=2&Clip=4&Category=Unit"
```
Expected: 4 pass, 0 fail / 10 pass, 0 fail / 8 pass, 0 fail

## Background for the Instructor
OrderLine already demonstrates the shadow FK pattern:
- No CLR `OrderId` property exists on OrderLine.
- EF config: `builder.HasMany(o => o.Lines).WithOne().HasForeignKey("OrderId").IsRequired();`
- Reading it: `ctx.Entry(line).Property<int>("OrderId").CurrentValue`

Clip 5 applies the same concept to Order.CustomerId:
- Remove the public (and later private set) CLR property entirely.
- EF maintains CustomerId entirely in shadow state. No CLR field or property of any kind exists on Order.
- Reading it externally requires the EF Entry API — same as OrderLine.OrderId.

## Student Changes

### Step 1 — Run the app to ensure a clean database
Before making any code changes, run the app once so it drops and recreates the database from the current migrations. This guarantees the schema is in a clean, known state before the migration in step 9 is applied.

Open a terminal in the **`ConsoleAppProject` folder** (the subfolder containing `ConsoleAppProject.csproj`), then run:
```powershell
dotnet run
```
Exit the app immediately after it launches.

### Step 2 — Remove the CLR CustomerId property from Order
- File: `OrderManagement.Domain/Aggregates/Order.cs`
- Delete: `//TODO: Module 2 Clip 5 — Completely delete the CustomerId property (making it a shadow property). Map the relationship with HasOne in OrderConfiguration instead (see the clip instructions).`
- Delete: `public int CustomerId { get; private set; }` — add nothing to replace it.

### Step 3 — Remove CustomerId from Order.Place()
- File: `OrderManagement.Domain/Aggregates/Order.cs`
- Method: `Place(int customerId, ...)`
- Delete these three lines:
  - `//TODO: Module 2 Clip 5 — After removing the CLR CustomerId property, update the object initializer:`
  - `// Remove: CustomerId = customerId,`
  - `// Add nothing. The entity does not store CustomerId. EF owns it through shadow state.`
- Delete: `CustomerId = customerId,` from the object initializer. Add nothing after it.
  The `customerId` parameter is still received by the method — EF will own its persistence via shadow state.

### Step 4 — Declare the shadow property in EF configuration
- File: `OrderManagement.Infrastructure/Configurations/OrderConfiguration.cs`
- Delete: `//TODO: Module 2 Clip 5 — After removing the CLR CustomerId property, replace this line`
- Delete: `builder.Property(o => o.CustomerId)` and `    .IsRequired();`
- Delete: `//  with:`
- Uncomment the block below (remove the `//` prefix from all lines):
  ```cs
  //builder.Property<int>("CustomerId");
  //builder.HasOne<Customer>()
  //       .WithMany()
  //       .HasForeignKey("CustomerId")
  //       .IsRequired();
  ```

### Step 5 — Fix SeedDataHelper to set the shadow property before saving
- File: `ConsoleAppProject/CodeAndDemonstrations/SeedDataHelper.cs`
- Method: `EnsureOrderWithLinesAsync`
- Find TODO marker: Module 2 Clip 5
- Action: Delete the TODO comment, then remove the `//` prefix from the line below it, so the block reads:
  ```cs
  ctx.Orders.Add(order);
  ctx.Entry(order).Property<int>("CustomerId").CurrentValue = CustomerAId;
  await ctx.SaveChangesAsync();
  ```
  Without this, EF cannot find a value for the shadow CustomerId and the save throws a NULL constraint violation.

### Step 6 — Set the shadow property on the eight seeded orders
- File: `ConsoleAppProject/CodeAndDemonstrations/SeedDataHelper.cs`
- Method: `SeedOrdersAsync`
- Find TODO marker: Module 2 Clip 5 (there are eight, one under each `context.Orders.Add(...)` call)
- Action: For each of the eight markers, delete the TODO comment and remove the `//` prefix from the line below it, so every seeded order reads:
  ```cs
  context.Orders.Add(order1);
  context.Entry(order1).Property<int>("CustomerId").CurrentValue = CustomerAId;
  ```
  Each order uses its own variable name and its own customer, so `order2` pairs with `CustomerAId`, `order3` with `CustomerBId`, and so on down the method.
  `SeedAsync` is the seeder Module 3 onward uses to fill the database with eight orders. Module 2 does not call it, so nothing here fails during this module, but leaving these lines commented breaks every later module's demos with a NULL constraint violation on the `CustomerId` column.

### Step 7 — Fix the other demo saves that also need the shadow property set

Each of the files below saves an order directly to the context. Without setting the shadow
CustomerId before `SaveChangesAsync`, EF has no value for the column and throws a NULL constraint
violation at runtime. Each location already has a `//TODO: Module 2 Clip 5` marker and a commented-out
fix line. The action for all three is the same: delete the TODO comment and uncomment the line below it.

**7a.**
- File: `ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstratePrivateSettersVsFields.cs`
- Method: `ScenarioAPrivateSetterAsync`
- Find TODO marker: Module 2 Clip 5 (just below `ctxA.Orders.Add(order);`)
- Action: Delete the TODO comment, then remove the `//` prefix from the line below it:
  ```cs
  ctxA.Entry(order).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
  ```

**7b.**
- File: `ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstratePrivateSettersVsFields.cs`
- Method: `ScenarioBBackingFieldAsync`
- Find TODO marker: Module 2 Clip 5 (just below `ctxB.Orders.Add(order);`)
- Action: Delete the TODO comment, then remove the `//` prefix from the line below it:
  ```cs
  ctxB.Entry(order).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
  ```

**7c.**
- File: `ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateOwnedEntities.cs`
- Method: `ShowMoneyMappingAsync`
- Find TODO marker: Module 2 Clip 5 (just below `ctx.Orders.Add(order);`)
- Action: Delete the TODO comment, then remove the `//` prefix from the line below it:
  ```cs
  ctx.Entry(order).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
  ```

### Step 8 — Remove the CustomerId display from the Clip 1 demo
- File: `ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateAnemicVsDomainModel.cs`
- Find TODO marker: Module 2 Clip 5
- Action: Delete the `$"CustomerId: {validOrder.CustomerId}"` line (and the two comment lines above it).
  After this change, `order.CustomerId` will not compile anywhere — which is the proof that the
  CLR property is gone.

### Step 9 — Update the CustomerId display in the Backing Fields demo
- File: `ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateBackingFields.cs`
- Find TODO marker: Module 2 Clip 5 (in the `Reloaded Order Values` `BoxedArrayWithTitle` block)
- Action: Replace the `order.CustomerId` line and the two TODO comment lines below it with:
  ```cs
  $"CustomerId: {ctx.Entry(order).Property<int>("CustomerId").CurrentValue}",
  ```

### Step 10 — Create and apply the shadow-FK migration

Adding `HasOne<Customer>().WithMany().HasForeignKey("CustomerId").IsRequired()` in Step 4 changes the EF model: the `CustomerId` column is now the FK side of a real relationship to the `Customers` table. Without a migration this FK constraint is absent from the database, which means the schema does not match the model.

- Action: Open a terminal in the **solution root** (the folder that contains the `.sln` file), then run:
  ```powershell
  dotnet ef migrations add AddCustomerFkToOrder --project OrderManagement.Infrastructure --startup-project ConsoleAppProject --context OrderingContext --output-dir Migrations/Ordering
  dotnet ef database update --project OrderManagement.Infrastructure --startup-project ConsoleAppProject --context OrderingContext
  ```
  > ⚠️ These commands **must be run from the solution root folder**. Running them from inside a project subfolder causes the error: *"Unable to retrieve project metadata. Ensure it's an SDK-style project."*
- The migration will add a foreign key constraint `FK_Orders_Customers_CustomerId` on the `Orders` table and a supporting index. No column is added or removed — only the constraint and index are new.

> ⚠️ **Always generate migrations using the EF Core CLI tooling. Never hand-write or manually edit migration files, Designer files, or the ModelSnapshot.**

### Step 11 — Unlock Part 3 of the demo to prove shadow access
- File: `ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateReadOnlyCollections.cs`
- Find TODO marker: Module 2 Clip 5
- Action: Delete the three-line TODO comment, then remove the `//` prefix from every line of the block
  below it, through the closing `//));`. That block is the `WaitForUserInput` separator, the Part 3
  section banner, and the `BoxedArrayWithTitle` call that reads
  `ctx.Entry(order).Property<int>("CustomerId").CurrentValue`.

## Key Teaching Points
- Shadow properties exist in the EF model and the DB column, but have no CLR member of any kind.
- EF reads and writes them entirely through shadow state in the change tracker — no CLR field of any kind.
- External code uses `ctx.Entry(entity).Property<int>("propName").CurrentValue` to read them.
- For LINQ queries, use `EF.Property<int>(o, "CustomerId")` — see Module 6 specs for examples.
- The Customer entity (in Customers table) exists and is usable via OrderingContext — a navigation
  property could be added to Order in a future refactor if direct access is needed.
