# Clip05 - Applying Specifications in the Repository
Write one more specification, then run four of them through the single `OrderRepository.ListAsync` method to show the method count stops growing. No changes to `OrderRepository` — `ListAsync` was implemented in Module 4 Clip 4 and is not edited again.

## Demo Class
- DemonstrateSpecificationEvaluator.ShowEvaluatorAsync

## Tests
- `OrderManagement.Tests/Application/SpecificationTests.cs` → `OrdersByStatusSpec_ReturnsOnlyOrdersWithThatStatus` and `MultiLineOrdersSpec_ReturnsOnlyOrdersWithMoreThanOneLine` (Trait Module 6, Clip 5). `OrdersByStatusSpec_ReturnsOnlyOrdersWithThatStatus` fails before this clip, because an empty specification stores no predicate and returns every order. Both are walked in Clip 7.

## Files To Edit
- OrderManagement.Infrastructure/Specifications/OrdersByStatusSpecification.cs
- ConsoleAppProject/CodeAndDemonstrations/Module06/DemonstrateSpecificationEvaluator.cs

Note: `MultiLineOrdersSpecification.cs` and `LargeOrdersSpecification.cs` already ship complete. Only `OrdersByStatusSpecification` has a TODO, so the clip writes one specification and runs four. Requires having completed `OrderRepository.ListAsync` (Module 4 Clip 4).

## What To Verify
- Screen 1 prints the whole body of `ListAsync` and the line naming what is absent from it: no `Where`, no `Include`, no `OrderBy`.
- Screen 2 runs all four specifications and prints: `OrdersByCustomerSpecification(customerId)` 4 orders, `OrdersByStatusSpecification(Shipped)` 1 order, `MultiLineOrdersSpecification()` 4 orders, `LargeOrdersSpecification(100m)` 4 orders.
- `OrderRepository.cs` is read on camera and never edited.
- The solution builds with zero compiler errors.

## Student Changes

### 1. Implement the predicate in OrdersByStatusSpecification
- File: OrderManagement.Infrastructure/Specifications/OrdersByStatusSpecification.cs
- Find TODO marker: `Module 6 Clip 5 — Uncomment the specification predicate below:`
- Action: Delete the TODO comment and remove the `//` prefix from the line below it:
  ```csharp
  Query.Where(o => o.Status == status);
  ```

### 2. Uncomment the total filter in LargeOrdersSpecification
- File: OrderManagement.Infrastructure/Specifications/LargeOrdersSpecification.cs
- Find TODO marker: `Module 6 Clip 5 — Uncomment the total filter below`
- Action: Delete the TODO comment and remove the `//` prefix from the line below it:
  ```csharp
  Query.Where(o => o.Total.Amount >= minimumTotal);
  ```
- The line ships commented because `Order.Total` is a `decimal` until Module 2 Clip 4 changes the type to `Money`. The starter has to compile before that change is made, so the filter waits here.

### 3. Unlock the four-specification screen
- File: ConsoleAppProject/CodeAndDemonstrations/Module06/DemonstrateSpecificationEvaluator.cs
- Find TODO marker: `Module 6 Clip 5 — remove this block and uncomment the four-spec block below`
- Action: Delete the TODO comment and the active "Not Yet Implemented" `Console.Write` call below it, then remove the `//` prefix from every line of the commented block beneath (from `var specs` through the closing of the `BoxedArrayWithTitle` call), normalizing indentation as you go.

## Not A Student Change — The WithSpecification Alternative
`Ardalis.Specification.EntityFrameworkCore` ships a `WithSpecification` extension on `IQueryable<T>` that delegates to the same `SpecificationEvaluator`, so `ListAsync` could have been written this way instead:

```csharp
//Note: Another common way to get the results uses WithSpecification
//      and recognizes that AsQueryable() is implicit on the dbContext:
//return await _ctx.Orders.WithSpecification(spec).ToListAsync(ct);
```

Same expression tree, same generated SQL, same single round trip. `OrderRepository.ListAsync` keeps the explicit `SpecificationEvaluator.Default.GetQuery` call so the evaluator is visible on screen while the clip explains it. Leave `ListAsync` alone — try the shorter form in your own copy afterward if you want to compare the SQL the two produce.
