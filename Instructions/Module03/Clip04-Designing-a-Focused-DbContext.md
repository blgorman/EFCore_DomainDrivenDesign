# Clip04 - Designing a Focused DbContext
Give `ShippingContext` its own `DbSet<ShipmentReadModel>` and configuration, then unlock the demo screen gated on that change.

## Demo Class
- DemonstrateFocusedDbContext.ShowOrderingContextAsync

## Tests
No unit tests are introduced in this clip.

## Files To Edit
- OrderManagement.Infrastructure/Data/ShippingContext.cs
- ConsoleAppProject/CodeAndDemonstrations/Module03/DemonstrateFocusedDbContext.cs

In `DemonstrateFocusedDbContext.cs`, reading top to bottom, you'll hit the two TODO markers in this order, matching the step numbers:
1. `Module 3 Clip 4 — Step 1: Delete this entire "Not Yet Implemented" box` — above the NTI box.
2. `Module 3 Clip 4 — Step 2: Uncomment this block after adding DbSet<ShipmentReadModel> and ApplyConfiguration to ShippingContext.` — above the commented Screen 4 code.

Do both — the file has no Screen 5 in this method; it ends right after the `WaitForUserInput` separator that follows Screen 4.

## What To Verify
- `shippingCtx.Model.GetEntityTypes()` returns exactly one type: `ShipmentReadModel`.
- Screen 4 runs live — no "Not Yet Implemented" box prints anywhere in the output.
- Only one `WaitForUserInput` pause occurs after Screen 4 (the separator already sitting active in the starter file, right after where the commented block ends) — not two.

## Student Changes

### 1. Add the DbSet to ShippingContext
- File: OrderManagement.Infrastructure/Data/ShippingContext.cs
- Find TODO marker: `Module 3 Clip 4 — Add a DbSet<ShipmentReadModel> so ShippingContext can query its lean projection.`
- Action: Uncomment `public DbSet<ShipmentReadModel> Shipments => Set<ShipmentReadModel>();` and delete the TODO comment.

### 2. Register the configuration
- File: OrderManagement.Infrastructure/Data/ShippingContext.cs
- Method: OnModelCreating
- Find TODO marker: `Module 3 Clip 4 — Register ShipmentReadModelConfiguration so EF maps ShipmentReadModel to the Orders table.`
- Action: Uncomment `modelBuilder.ApplyConfiguration(new ShipmentReadModelConfiguration());` and delete the TODO comment.

### 3. Delete the "Not Yet Implemented" box
- File: ConsoleAppProject/CodeAndDemonstrations/Module03/DemonstrateFocusedDbContext.cs
- Method: ShowOrderingContextAsync
- Find TODO marker: `Module 3 Clip 4 — Step 1: Delete this entire "Not Yet Implemented" box`
- Action: Delete the TODO comment and the entire NTI box that follows it (the `SectionBanner("ShippingContext — Registered Entity Types")` call through the closing `));` of its `BoxedArrayWithTitle`).

### 4. Uncomment the Screen 4 block
- File: same
- Find TODO marker: `Module 3 Clip 4 — Step 2: Uncomment this block after adding DbSet<ShipmentReadModel> and ApplyConfiguration to ShippingContext.`
- Action: Remove the `//` prefix from every line in the commented block below this marker (the `shippingCtx.Model.GetEntityTypes()` query and its two output boxes). Delete the TODO comment. Screen 4 is the last screen in this method, so nothing follows it, and no `WaitForUserInput` separator belongs after it.
