# Clip05 - One Table, Two Models
Configure `ShipmentReadModelConfiguration` so ShippingContext never touches the Orders schema, matches OrderingContext's Status column type, then unlock the live cross-context query screen.

## Demo Class
- DemonstrateOneTableTwoModels.ShowBothContextsAsync

## Tests

No unit tests are introduced in this clip. One integration test starts passing here.

**Integration test (requires Docker):**
```powershell
dotnet test --filter "Module=3&Clip=5&Category=Integration"
```
Expected before: 1 fail. Expected after: 1 pass.

`BoundedContextTests.ShipmentReadModel_ReadsOrderPlacedViaOrderingContext` writes an order through
`OrderingContext` and reads the same row back through `ShippingContext`. It needs both this clip and
Clip 4: without Clip 4 it fails with *Cannot create a DbSet for 'ShipmentReadModel'*, and with Clip 4
alone it fails with *Unable to cast object of type 'System.String' to type 'System.Int32'* — the
`Status` column is stored as a string by `OrderConfiguration`, and Step 2 below is what teaches
`ShipmentReadModelConfiguration` to read it that way.

## Files To Edit
- OrderManagement.Infrastructure/Configurations/ShipmentReadModelConfiguration.cs
- ConsoleAppProject/CodeAndDemonstrations/Module03/DemonstrateOneTableTwoModels.cs

## Prerequisite
Clip 4 must be complete (ShippingContext needs its DbSet and configuration registered).

## What To Verify
- ShippingContext no longer risks generating migrations for the Orders table.
- ShippingContext's Status column matches OrderingContext's string conversion.
- Screen 5 (live query against both contexts) runs live instead of being commented out.

## Student Changes

### 1. Exclude the Orders table from ShippingContext's migrations
- File: OrderManagement.Infrastructure/Configurations/ShipmentReadModelConfiguration.cs
- Method: Configure
- Find TODO marker: `Module 3 Clip 5 — Replace the bare ToTable with ExcludeFromMigrations so ShippingContext never generates schema changes for the Orders table (owned by OrderingContext).`
- Action: Delete the active line
  ```cs
  builder.ToTable("Orders");
  ```
  Uncomment the replacement above it:
  ```cs
  builder.ToTable("Orders", t => t.ExcludeFromMigrations());
  ```
  Delete the TODO comment.

### 2. Match the Status column type
- File: same
- Find TODO marker: `Module 3 Clip 5 — Add HasConversion<string>() and HasMaxLength(20) to match OrderConfiguration's Status column type (nvarchar, not int).`
- Action: Delete the redundant active line
  ```cs
  builder.Property(s => s.Status).IsRequired();
  ```
  so only the conversion-configured version remains:
  ```cs
  builder.Property(s => s.Status)
      .IsRequired()
      .HasConversion<string>()
      .HasMaxLength(20);
  ```
  Delete the TODO comment.

### 3. Unlock Screen 5 in DemonstrateOneTableTwoModels
- File: ConsoleAppProject/CodeAndDemonstrations/Module03/DemonstrateOneTableTwoModels.cs
- Find TODO marker: `Module 3 Clip 5 — Step 1: Delete this entire "Not Yet Implemented" box`
- Action: Delete the TODO comment and the entire NTI box that follows it.
- Find TODO marker: `Module 3 Clip 5 — Step 2: Uncomment this block after ShipmentReadModelConfiguration calls ExcludeFromMigrations() and matches the Status column conversion.`
- Action: Remove the `//` prefix from every line in the Screen 5 block. Delete the TODO comment. A single active `WaitForUserInput` separator already sits after this block, leading into Screen 6 — it does not need to be added or removed.
