# Clip08 - Factory Methods and Guard Clauses
Guards reject invalid input and preserve invariant safety.

## Demo Class
- DemonstrateFactoryAndGuards.ShowFactoryAndGuardsAsync

## Files To Edit
- OrderManagement.Domain/ValueObjects/Money.cs
- OrderManagement.Domain/Aggregates/OrderLine.cs
- OrderManagement.Domain/Aggregates/Order.cs
- ConsoleAppProject/CodeAndDemonstrations/DemonstrateFactoryAndGuards.cs

## What To Verify
- Invalid inputs throw expected exceptions.
- Valid path still creates order successfully.
- Order.Total is computed from line totals and stays synchronized after AddLine.

## Tests

**Before implementing this clip:**
```powershell
dotnet test --filter "Module=2&Clip=8&Category=Unit"
```
Expected: 2 pass, 13 fail (of 15 total: 13 input validation guards + 2 state-transition guards)

**After implementing this clip:**
```powershell
dotnet test --filter "Module=2&Clip=8&Category=Unit"
```
Expected: 15 pass, 0 fail

**Regression — prior clips should still pass:**
```powershell
dotnet test --filter "Module=2&Clip=2&Category=Unit"
dotnet test --filter "Module=2&Clip=3&Category=Unit"
dotnet test --filter "Module=2&Clip=4&Category=Unit"
```
Expected: 4 pass, 0 fail / 10 pass, 0 fail / 8 pass, 0 fail

## Student Changes
1. Add guards in Money.Create
	- File: OrderManagement.Domain/ValueObjects/Money.cs
	- Method: Create(decimal amount, string currency)
	- Find TODO marker: Module 2 Clip 8 - Add guard clauses.
	- Action: Delete the TODO comment, then uncomment all validation checks below it.

2. Add guard in Money.Add
	- File: OrderManagement.Domain/ValueObjects/Money.cs
	- Method: Add(Money other)
	- Find TODO marker: Module 2 Clip 8 - Add guard clause.
	- Action: Delete the TODO comment, then uncomment the currency mismatch guard below it.

3. Add guard in Money.Multiply
	- File: OrderManagement.Domain/ValueObjects/Money.cs
	- Method: Multiply(int quantity)
	- Find TODO marker: Module 2 Clip 8 - Add guard clause.
	- Action: Delete the TODO comment, then uncomment the negative quantity guard below it.

4. Add guards in OrderLine constructor
	- File: OrderManagement.Domain/Aggregates/OrderLine.cs
	- Method: OrderLine(int productId, int quantity, Money unitPrice)
	- Find TODO marker: Module 2 Clip 8 - Add guard clauses.
	- Action: Delete the TODO comment, then uncomment the quantity and unitPrice null checks below it.

5. Add guards in Order.Place
	- File: OrderManagement.Domain/Aggregates/Order.cs
	- Method: Place(int customerId, IEnumerable<(int productId, int quantity, Money unitPrice)> lines)
	- There are three separate TODO markers in this method:
	  - `Module 2 Clip 8 — Customer ID guard` — delete the marker, uncomment the customerId check.
	  - `Module 2 Clip 8 — Lines cannot be null` — delete the marker, uncomment the ThrowIfNull call.
	  - `Module 2 Clip 8 — Lines List can't be empty` — delete the marker, uncomment the Count == 0 check.
	- Note: `var linesList = lines.ToList();` is already active between the null and count guards — do not add or remove it.

6. Add duplicate-product guard in Order.AddLine
	- File: OrderManagement.Domain/Aggregates/Order.cs
	- Method: AddLine(int productId, int quantity, Money unitPrice)
	- Find TODO marker: Module 2 Clip 8 - Add guard clauses (uses _lines, complete Clip 2 first).
	- Action: Delete the TODO comment, then uncomment the productId guard and duplicate-product guard below it.

7. Add state-transition guards in Order domain methods
	- File: OrderManagement.Domain/Aggregates/Order.cs
	- Methods: Process, Confirm, Ship, Cancel
	- Find TODO marker: Module 2 Clip 8 — Add a state-transition guard (or guards for Cancel).
	- Action: In each method, delete the TODO comment and uncomment the guard block below it.
	- These enforce that domain methods are called in legal order:
	  - Process requires Placed
	  - Confirm requires Processing
	  - Ship requires Confirmed
	  - Cancel rejects Shipped and already-Cancelled states

8. Compute aggregate Total in Order.Place
	- File: OrderManagement.Domain/Aggregates/Order.cs
	- Method: Place(int customerId, IEnumerable<(int productId, int quantity, Money unitPrice)> lines)
	- Find TODO marker: `Module 2 Clip 8 — Delete the Money.Create(0m,...) line above`
	- Delete: `order.Total = Money.Create(0m, linesList[0].unitPrice.Currency);`
	- Delete the TODO comment, then uncomment: `//order.Total = Money.Create(order.Lines.Sum(l => l.LineTotal.Amount), linesList[0].unitPrice.Currency);`

9. Keep Total synchronized in Order.AddLine
	- File: OrderManagement.Domain/Aggregates/Order.cs
	- Method: AddLine(int productId, int quantity, Money unitPrice)
	- Find TODO marker: Module 2 Clip 8 - Keep Total synchronized after AddLine.
	- Action: Delete the TODO comment, then uncomment the line below it and keep it active:
	  ```cs
	  Total = Money.Create(Lines.Sum(l => l.LineTotal.Amount), unitPrice.Currency);
	  ```

10. Update the display in `DemonstrateOwnedEntities.cs`
- File: `ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateOwnedEntities.cs`
- Find and delete this line:
  ```cs
  "After Clip 8, total will be calculated from line totals. Until then, it is 0.",
  ```

11. Re-enable guard demos in DemonstrateFactoryAndGuards
	- File: ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateFactoryAndGuards.cs
	- Find all TODO markers: Module 2 Clip 8 - Uncomment after adding guard clauses ...
	- Action: For each marker, delete the TODO comment and uncomment the demo block below it.
	- **Note:** The `customerId` argument in the `Order.Place` calls inside these blocks is `Random.Shared.Next(1, int.MaxValue)` rather than `SeedDataHelper.CustomerAId`. This is intentional — `DemonstrateFactoryAndGuards` has no `IServiceProvider` and cannot seed the database. Using a random positive integer ensures the correct guard fires rather than the `customerId <= 0` guard, which would mask the guard being demonstrated.
	- In Part 5, find the TODO marker: `Module 2 Clip 8` inside the BoxedArrayWithTitle string array.
	- Action: Delete the TODO placeholder string line, then uncomment the `$"Total:..."` line immediately below it.


