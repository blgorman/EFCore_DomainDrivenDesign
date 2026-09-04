# Clip02 - Private Backing Fields and HasField Configuration
Introduce the private `_lines` backing field so the collection is mutation-controlled, then wire EF to use it.

## Tests

**Before implementing this clip:**
```powershell
dotnet test --filter "Module=2&Clip=2&Category=Unit"
```
Expected: 3 pass, 1 fail
- `Lines_IsReadOnly_CannotBeDirectlyMutated` — FAIL (`Lines` is a mutable `List<T>` until the backing field is added)

**After implementing this clip:**
```powershell
dotnet test --filter "Module=2&Clip=2&Category=Unit"
```
Expected: 4 pass, 0 fail

**Integration tests (requires Docker):**
```powershell
dotnet test --filter "Module=2&Clip=2&Category=Integration"
```
Expected before: 1 pass, 1 fail
Expected after:  2 pass, 0 fail

- `Order_Lines_AreReadOnly_AfterRoundTrip` — **FAILS** before Clip 2 (`List<T>.IsReadOnly = false`). Passes after.
- `Order_Lines_LoadedFromDatabase_ViaHasFieldConfig` — Passes before Clip 2 (EF writes to the public `List<T>` directly) and passes after. `_lines` matches EF Core's `_<camelCase>` backing-field convention, so EF Core resolves the navigation to the backing field with or without step 3. The `HasField` call states that mapping explicitly rather than leaving it to the field name.

## Demo Class
- DemonstrateBackingFields.ShowBackingFieldConfigAsync

## Files To Edit
- OrderManagement.Domain/Aggregates/Order.cs
- OrderManagement.Infrastructure/Configurations/OrderConfiguration.cs
- ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateBackingFields.cs

## Plumbing Note
- Demo already seeds via SeedDataHelper.EnsureOrderWithLinesAsync.
- Do not switch to SeedDataHelper.SeedAsync in Module 2.

## What To Verify
- order.Lines returns IReadOnlyCollection<OrderLine> — callers cannot call Lines.Add.
- AddLine() path shows line count change (writing to _lines).
- Reloaded order shows correct Lines count from the _lines backing field.
- The "What Doesn't Compile" screen is optional and is covered by Clip 2 Step 5. Skipping Clip 2 Step 5 changes nothing else in the demo.

## Student Changes

### 1. Replace the public List with a private backing field
- File: OrderManagement.Domain/Aggregates/Order.cs
- Find TODO marker: `Module 2 Clip 2 — Replace this line`
- Action: Delete the TODO comment, the `public List<OrderLine> Lines { get; set; } = new();` line below it, and the `//  with:` comment. Then remove the `//` prefix from the two lines beneath, leaving:
  ```cs
  private readonly List<OrderLine> _lines = new();
  public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();
  ```

### 2. Write new lines through the backing field
- File: OrderManagement.Domain/Aggregates/Order.cs
- Method: AddLine(int productId, int quantity, Money unitPrice)
- Action, two edits:
  1. Delete line 73, the `//TODO: Module 2 Clip 2 — Change Lines.Add to _lines.Add...` comment. It sits at line 73 once step 1 is done, because step 1 removed three lines above it. Leave the `//TODO: Module 2 Clip 8` comment on the line below alone.
  2. On the line that now reads `Lines.Add(new OrderLine(productId, quantity, unitPrice));`, change `Lines.Add` to `_lines.Add`.

### 3. Wire EF to use the backing field for the Lines navigation
- File: OrderManagement.Infrastructure/Configurations/OrderConfiguration.cs
- Method: Configure(EntityTypeBuilder<Order> builder)
- Action: Delete lines 42 through 44 and put these two lines in their place:
  ```cs
  builder.Navigation(o => o.Lines)
          .HasField("_lines");
  ```

### 4. Replace the NTI block and uncomment the full demo body
- File: ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateBackingFields.cs
- Action:
  1. Delete lines 14 through 27 — the entire "Not Yet Implemented" box, from the `SectionBanner("Backing Field Configuration")` line down to and including the `));` that closes it.
  2. Delete line 29, the `//TODO: Module 2 Clip 2 — Uncomment ONLY after...` comment.
  3. Remove the `//` prefix from lines 31 through 121, the remaining commented-out demo body.
- The two `// order.Lines.Add(...)` and `// var line = new OrderLine(...)` lines inside the body are intentional compile errors — they are permanently commented and must never be uncommented.

### 5. Optional — show the "What Doesn't Compile" screen

This step is optional. The demo runs correctly without it.

- File: ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateBackingFields.cs
- Action: Delete line 48, the `//TODO: Module 2 Clip 2 — OPTIONAL: ...` comment, then remove the `//` prefix from lines 49 through 65. These are the line numbers after Clip 2 Steps 1 through 4 are complete.
- Result: the demo prints one extra screen naming the two compiler errors that the previous two commented lines would produce — `CS1061` because `IReadOnlyCollection` has no `Add`, and `CS0122` because the `OrderLine` constructor is internal.
- Leave the two `// order.Lines.Add(...)` and `// var line = new OrderLine(...)` lines commented. This step adds a screen describing those errors; it does not enable the errors themselves.
