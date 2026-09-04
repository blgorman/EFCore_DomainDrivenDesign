# Clip03 - Private Setters: Enforcing Encapsulation
Demonstrate private setters as a compile-time encapsulation strategy for scalar properties.

## Demo Class
- DemonstratePrivateSettersVsFields.CompareApproachesAsync

## Files To Edit
- OrderManagement.Domain/Aggregates/Order.cs
- ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstratePrivateSettersVsFields.cs

## What To Verify
- After making Status private set, `order.Status = OrderStatus.Shipped` in the demo is a compile error — comment it out.
- The 4 new reflection tests flip from fail to pass, proving the setter is private.
- The demo output correctly states that Status can only be set by domain methods.

## Tests

Four reflection-based tests enforce the private setter at the type level — they fail before this clip and pass after.

```powershell
dotnet test --filter "Module=2&Clip=3&Category=Unit"
```
Expected before: 6 pass, 4 fail  
Expected after:  10 pass, 0 fail

**Regression — prior clips should still pass:**
```powershell
dotnet test --filter "Module=2&Clip=2&Category=Unit"
```
Expected: 4 pass, 0 fail

## Student Changes
1. Make Order state writable only from domain methods
	- File: OrderManagement.Domain/Aggregates/Order.cs
	- Delete: `//TODO: Module 2 Clip 3 — Change all public setters to private set (Status, PlacedAt, Id, Total, and CustomerId).`
	- Change these properties from public set to private set:
	  - Id
	  - CustomerId
	  - Status
	  - PlacedAt
	  - Total

2. Fix compile error and update the demo proof
	- File: ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstratePrivateSettersVsFields.cs
	- Method: ScenarioAPrivateSetterAsync
	- Step A: Find the TODO marker `Module 2 Clip 3 — After making Status private set, comment this line out`.
	          Delete that TODO comment, then comment out the active `order.Status = OrderStatus.Shipped;` line below it — it is now a compile error.
	- Step B: Find the first TODO marker `Module 2 Clip 3 — After making Status private set, uncomment these two lines and delete the placeholder below`.
	          Delete that TODO comment, uncomment the two lines beneath it, and delete the placeholder line that follows.
	- Step C: Find the second TODO marker `Module 2 Clip 3 — After making Status private set, uncomment the line below and delete the placeholder`.
	          Delete that TODO comment, uncomment the line beneath it, and delete the placeholder line that follows.

3. End-to-end demo note
	- File: ConsoleAppProject/CodeAndDemonstrations/Module02/DemonstrateOrderAggregate.cs
	- Action: No required edits in Clip03. Do not modify this end-to-end demo file until Clip09.
