# Clip07 - Testing Specifications in Isolation
Read the existing specification tests, then write two more. In-memory unit tests already cover `OrdersByStatusSpecification`, `MultiLineOrdersSpecification`, and `OrderSearchSpecification` via `spec.Evaluate`. This clip adds a test for `LargeOrdersSpecification`, and a test that proves where in-memory evaluation stops working.

## Demo Class
None — this clip lives in the `OrderManagement.Tests` project. There is no console menu entry.

## Tests
- `OrderManagement.Tests/Application/SpecificationTests.cs` → `OrdersByStatusSpec_ReturnsOnlyOrdersWithThatStatus`, `MultiLineOrdersSpec_ReturnsOnlyOrdersWithMoreThanOneLine` (unit, in-memory, Clip 5 specifications), plus `OrderSearchSpec_WithStatusFilter_FiltersCorrectly` and `OrderSearchSpec_WithNoFilters_ReturnsAll` (unit, in-memory, Clip 6 specification)
- `OrderManagement.Tests/Integration/OrderRepositoryTests.cs` → `ListAsync_WithOrdersByCustomerSpec_ReturnsOnlyThatCustomersOrders` (integration, real SQL Server)

## Files To Edit
- OrderManagement.Tests/Application/SpecificationTests.cs

Note: prerequisites — Clip 5 (`OrdersByStatusSpecification`) and Clip 6 (`OrderSearchSpecification`) for the unit tests; Clip 2 (predicate) plus Module 4 Clip 4 (repository) and a reachable SQL Server for the integration test. `OrdersByCustomerSpecification` cannot be evaluated in memory because `EF.Property<int>(o, "CustomerId")` only resolves inside an EF Core query — the comment at the top of `SpecificationTests.cs` records this boundary. Three specifications filter on CLR properties and test with no database; one filters on a shadow property and needs SQL Server. That split is the clip.

## What To Verify
- `dotnet test --filter "Module=6&Category=Unit"` — all six unit tests pass in milliseconds with no database.
- `dotnet test --filter "Module=6"` — all seven tests pass (the integration test hits SQL Server through `MsSqlFixture`).
- `OrdersByCustomerSpec_CannotBeEvaluatedInMemory` passes by catching the exception, so a green run is proof of the boundary rather than a description of it.
- The status-filter test builds real aggregates through `Order.Place` and walks state transitions (`Process`, `Confirm`, `Ship`) rather than setting a status directly — the domain guards from Module 2 still apply inside tests.

## Student Changes

Both steps below are in OrderManagement.Tests/Application/SpecificationTests.cs.

### 1. Write the LargeOrdersSpecification test
- Find TODO marker: `Module 6 Clip 7 — Uncomment the LargeOrdersSpecification test below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line of the test beneath it, normalizing indentation as you go, so the test reads:
  ```csharp
  [Fact]
  [Trait("Clip", "7")]
  public void LargeOrdersSpec_ReturnsOnlyOrdersAtOrAboveTheMinimum()
  {
      // CreateOrder gives every line a quantity of 1 at 10.00, so the total is 10.00 per line.
      var smallOrder = CreateOrder(OrderStatus.Placed, lineCount: 1);   // total 10.00
      var largeOrder = CreateOrder(OrderStatus.Placed, lineCount: 3);   // total 30.00
      var allOrders  = new List<Order> { smallOrder, largeOrder };

      var largeOrdersSpec = new LargeOrdersSpecification(25m);
      var largeOrders     = largeOrdersSpec.Evaluate(allOrders).ToList();

      largeOrders.ShouldHaveSingleItem();
      largeOrders[0].Total.Amount.ShouldBe(30m);
  }
  ```
- `CreateOrder` gives every line a quantity of 1 at 10.00, so a one-line order totals 10.00 and a three-line order totals 30.00. A minimum of 25.00 returns the three-line order alone.

### 2. Write the shadow-property boundary test
- Find TODO marker: `Module 6 Clip 7 — Uncomment the shadow-property boundary test below:`
- Action: Delete the TODO comment and remove the `//` prefix from every line of the test beneath it, normalizing indentation as you go, so the test reads:
  ```csharp
  [Fact]
  [Trait("Clip", "7")]
  public void OrdersByCustomerSpec_CannotBeEvaluatedInMemory()
  {
      var allOrders            = new List<Order> { CreateOrder(OrderStatus.Placed) };
      var ordersByCustomerSpec = new OrdersByCustomerSpecification(1);

      var evaluationException = Should.Throw<InvalidOperationException>(
          () => ordersByCustomerSpec.Evaluate(allOrders).ToList());
      evaluationException.Message.ShouldContain("may only be used within Entity Framework LINQ queries");
  }
  ```
- The test asserts that `Evaluate` throws an `InvalidOperationException` whose message reads "The EF.Property<T> method may only be used within Entity Framework LINQ queries." That is the boundary between the three specifications that unit test and the one that needs SQL Server.
