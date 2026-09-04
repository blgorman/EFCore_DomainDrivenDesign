# Clip04 - Owned Entities: Converting Order.Total from decimal to Money

Demonstrate why a plain `decimal Total` is insufficient, then convert it to a `Money` value object mapped with `OwnsOne`. The key insight: the `Total` column already exists — only a `Currency` column is added, so the schema change is minimal.

## Demo Class
- DemonstrateOwnedEntities.ShowMoneyMappingAsync

## Files To Edit
- OrderManagement.Domain/Aggregates/Order.cs
- OrderManagement.Infrastructure/Configurations/OrderConfiguration.cs
- ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateAnemicVsDomainModel.cs
- ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateOwnedEntities.cs

## What To Verify
- Order.Total is typed as Money (not decimal) after student changes.
- Amount and Currency round-trip through the database.
- The Orders table gains one new column (Currency) — Total column name is unchanged.

## Tests

**Before implementing this clip:**
```powershell
dotnet test --filter "Module=2&Clip=4&Category=Unit"
```
Expected: 5 pass, 3 fail

The 5 that pass are regression checks on the `Money` value object (`Create`, `Add`, `Multiply`) — those are pre-wired in the starter.
The 3 that fail are gating tests that assert `Order.Total` is typed as `Money`. They compile in the starter state (using reflection and the `(object)order.Total` boxing trick) but fail at runtime because `Total` is still `decimal`.

**After implementing this clip:**
```powershell
dotnet test --filter "Module=2&Clip=4&Category=Unit"
```
Expected: 8 pass, 0 fail

**Regression — prior clips should still pass:**
```powershell
dotnet test --filter "Module=2&Clip=2&Category=Unit"
dotnet test --filter "Module=2&Clip=3&Category=Unit"
```
Expected: 4 pass, 0 fail / 10 pass, 0 fail

## Background for the Instructor
`Order.Total` starts as `decimal` — just a raw number with no currency context. After this clip, it becomes a `Money` value object. EF persists it with `OwnsOne`, storing Amount and Currency inline in the Orders table (no separate table, no join).

By configuring `HasColumnName("Total")` for the Amount property, the existing column keeps its name. Only `Currency` is new. The migration is intentionally small to reinforce that EF OwnsOne is a mapping concern, not a schema overhaul.

## Student Changes

### 1. Run the app to ensure a clean database
Before making any code changes, run the app once so it drops and recreates the database from the current migrations. This guarantees the schema is in a clean, known state before the migration in step 7 is applied.

Open a terminal in the **`ConsoleAppProject` folder** (the subfolder containing `ConsoleAppProject.csproj`), then run:
```powershell
dotnet run
```
Exit the app immediately after it launches.

### 2. Change Order.Total from decimal to Money
- File: OrderManagement.Domain/Aggregates/Order.cs
- Delete: `//TODO: Module 2 Clip 4 — Change Total from decimal to Money and map with OwnsOne in OrderConfiguration.`
- Change the property:
  ```cs
  public decimal Total { get; private set; }
  ```
  to:
  ```cs
  public Money Total { get; private set; }
  ```

### 3. Fix the private constructor
- File: OrderManagement.Domain/Aggregates/Order.cs
- Find TODO marker: `Module 2 Clip 4 — After changing Total to Money, replace this line`
- Action: Delete the TODO comment, the `private Order() { }` line below it, and the `//  with:` comment. Then remove the `//` prefix from the line beneath, leaving:
  ```cs
  private Order() { Total = null!; }
  ```
- The two `//Module 2 Clip 7` comment lines above stay where they are, directly above the remaining constructor.

### 4. Update the Total initialization in Place()
- File: OrderManagement.Domain/Aggregates/Order.cs
- Inside `Place()`, find this block and make these changes:
  - Delete: `//TODO: Module 2 Clip 4 — After changing Total to Money, replace this line`
  - Delete: `order.Total = 0.0m;`
  - Delete: `//  with:`
  - Uncomment: `//order.Total = Money.Create(0m, linesList[0].unitPrice.Currency);`
  > Leave the `//TODO: Module 2 Clip 8` comment and its commented-out line in place.

### 5. Replace the decimal Total mapping with OwnsOne
- File: OrderManagement.Infrastructure/Configurations/OrderConfiguration.cs
- Delete: `//TODO: Module 2 Clip 4 — After changing Order.Total from decimal to Money, replace this line`
- Delete: `builder.Property(o => o.Total)` and `    .IsRequired();`
- Delete: `//  with:`
- Uncomment the `builder.OwnsOne` block (remove the `//` prefix from all lines)

### 6. Fix compile error in DemonstrateAnemicVsDomainModel.cs
- File: ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateAnemicVsDomainModel.cs
- Find TODO marker: `Module 2 Clip 4 — After changing Total to Money, this becomes a compile error:`
- After changing `Order.Total` to `Money`, the active line below that TODO is a compile error because `Money` does not implement `:C` format.
- Action:
  1. Delete the `//TODO: Module 2 Clip 4` comment line
  2. Delete the active `$"Total:      {validOrder.Total:C}  <-- always 0 until Clip 8 adds the calculation"` line
  3. Uncomment: `//$"Total:      {validOrder.Total.Amount:C}  <-- always 0 until Clip 8 adds the calculation"`
  4. Delete the `// Change to:` comment line

### 7. Create and apply the Clip 4 migration
- Action: Open a terminal in the **solution root** (the folder that contains the `.sln` file), then run:
  ```powershell
  dotnet ef migrations add AddCurrencyToOrder --project OrderManagement.Infrastructure --startup-project ConsoleAppProject --context OrderingContext --output-dir Migrations/Ordering
  dotnet ef database update --project OrderManagement.Infrastructure --startup-project ConsoleAppProject --context OrderingContext
  ```
  > ⚠️ These commands **must be run from the solution root folder**. Running them from inside a project subfolder (e.g. `OrderManagement.Infrastructure`) causes the error: *"Unable to retrieve project metadata. Ensure it's an SDK-style project."*
- The migration will add only the `Currency` column. The `Total` column is unchanged (same name, same type) because `HasColumnName("Total")` maps Amount to the existing column.

> ⚠️ **Always generate migrations using the EF Core CLI tooling. Never hand-write or manually edit migration files, Designer files, or the ModelSnapshot.** Hand-edited snapshots can diverge from what EF Core would generate, causing spurious or broken migrations on subsequent `dotnet ef migrations add` runs.
>
> **Option 1 — EF Core CLI** (VS Code, terminal, or PowerShell outside VS):
> Requires the EF Core CLI tools. If you haven't installed them, run this once:
> ```
> dotnet tool install --global dotnet-ef
> ```
> Then add the migration:
> ```
> dotnet ef migrations add <MigrationName> \
>     --context <ContextName> \
>     --project OrderManagement.Infrastructure \
>     --startup-project ConsoleAppProject \
>     --output-dir Migrations/<FolderName>
> ```
>
> **Option 2 — Package Manager Console** (Visual Studio):
> In the PMC dropdown, **set the Default Project to `OrderManagement.Infrastructure`** before running:
> ```
> Add-Migration <MigrationName> -Context <ContextName> -OutputDir Migrations/<FolderName>
> ```
>
> Verify the generated `.cs`, `.Designer.cs`, and `ModelSnapshot.cs` files before moving on.

### 8. Update the display in `DemonstrateOwnedEntities.cs`
- File: `ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateOwnedEntities.cs`

**Substitution 1 — section banner:**
- Delete: `//TODO: Module 2 Clip 4 - Replace this line:`
- Delete: `OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Order.Total — Starter (decimal)"), ConsoleColor.DarkBlue);`
- Delete: `//WITH:`
- Uncomment (remove `//`): `//OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Order.Total — After Clip 4 (Money with OwnsOne)"), ConsoleColor.DarkBlue);`

**Substitution 2 — BoxedArrayWithTitle call:**
- Delete: `//TODO: Module 2 Clip 4 - Replace this line:`
- Delete: `Console.Write(OutputHelpers.BoxedArrayWithTitle(`
- Delete: `    "Order saved and reloaded — Total as plain decimal",`
- Delete: `//WITH:`
- Uncomment (remove `//`): `//Console.Write(OutputHelpers.BoxedArrayWithTitle(`
- Uncomment (remove `//`): `    //"Order saved and reloaded — Total as Money (OwnsOne)",`

**Substitution 3 — Total line inside the array:**
- Delete: `//TODO: Module 2 Clip 4 — After changing Total to Money, change this line`
- Delete: `$"Total:       {reloaded.Total:F2}  <-- decimal only, no currency attached",`
- Delete: `//to:`
- Uncomment (remove `//`): `//$"Total.Amount:    {reloaded.Total.Amount:F2}",`
- Uncomment (remove `//`): `//$"Total.Currency:  {reloaded.Total.Currency}",`

## Key Teaching Points
- `decimal` carries no currency — `Money` makes currency explicit and type-safe.
- `OwnsOne` stores the value object inline in the owning table — no join, no extra table.
- `HasColumnName` lets you reuse an existing column name, keeping the migration minimal.
- The schema change is additive: one new column (`Currency`). The `Total` column is untouched.
