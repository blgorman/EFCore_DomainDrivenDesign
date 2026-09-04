# Clip02 - The Specification Pattern: Core Concept
Implement the `Query.Where` predicate in `OrdersByCustomerSpecification`, then unlock both demo screens: the one that inspects what the constructor recorded, and the one that runs the specification and prints the SQL and the rows.

## Demo Class
- DemonstrateSpecificationBase.ShowBaseSpecAsync

## Tests
- `OrderManagement.Tests/Integration/OrderRepositoryTests.cs` → `ListAsync_WithOrdersByCustomerSpec_ReturnsOnlyThatCustomersOrders` (Trait Module 6, Clip 2).

## Files To Edit
- OrderManagement.Infrastructure/Specifications/OrdersByCustomerSpecification.cs
- ConsoleAppProject/CodeAndDemonstrations/Module06/DemonstrateSpecificationBase.cs

Note: leave the Clip 4 marker in `OrdersByCustomerSpecification.cs` untouched; `Include`/`OrderBy` arrive in Clip 4.

## What To Verify
- Screen 1 prints `WhereExpressions count: 1`, `IncludeExpressions count: 0`, `OrderExpressions count: 0`.
- Screen 2 prints the SQL with a `WHERE` on the `CustomerId` column, then 4 orders for Customer A.
- The predicate uses `EF.Property<int>(o, "CustomerId")` because `CustomerId` has been a shadow property since Module 2 Clip 5.
- The solution builds with zero compiler errors.

## Student Changes

### 1. Implement the predicate in OrdersByCustomerSpecification
- File: OrderManagement.Infrastructure/Specifications/OrdersByCustomerSpecification.cs
- Find TODO marker: `Module 6 Clip 2 — Uncomment the specification predicate below:`
- Action: Delete the TODO comment and remove the `//` prefix from the line below it:
  ```csharp
  Query.Where(o => EF.Property<int>(o, "CustomerId") == customerId);
  ```

### 2. Unlock the internals screen
- File: ConsoleAppProject/CodeAndDemonstrations/Module06/DemonstrateSpecificationBase.cs
- Find TODO marker: `Module 6 Clip 2 — remove this block and uncomment the internals block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath (from `var whereCount` through the closing of the `BoxedArrayWithTitle` call), normalizing indentation as you go.

### 3. Unlock the execute screen
- File: ConsoleAppProject/CodeAndDemonstrations/Module06/DemonstrateSpecificationBase.cs
- Find TODO marker: `Module 6 Clip 2 — remove this block and uncomment the execute block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath (from `var sql` through the closing of the `BoxedArrayWithTitle` call), normalizing indentation as you go.
