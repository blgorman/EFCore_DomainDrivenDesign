using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Specifications;

namespace OrderManagement.Tests.Application;

// Note: OrdersByCustomerSpecification uses EF.Property<int>(o, "CustomerId") which cannot be
// evaluated in-memory via spec.Evaluate(). Its correctness is covered by the integration test
// ListAsync_WithOrdersByCustomerSpec_ReturnsOnlyThatCustomersOrders in OrderRepositoryTests.cs.
// Every other Module 6 specification filters on a CLR property, so spec.Evaluate() works and
// no database is required.

[Trait("Category", "Unit")]
[Trait("Module", "6")]
public class SpecificationTests
{
    private static Order CreateOrder(OrderStatus status, int lineCount = 1)
    {
        var lines = Enumerable.Range(1, lineCount)
            .Select(_ => (Random.Shared.Next(1, int.MaxValue), 1, Money.Create(10m, "USD")))
            .ToArray();

        var order = Order.Place(Random.Shared.Next(1, int.MaxValue), lines);

        if (status == OrderStatus.Cancelled)
            order.Cancel();
        else if (status == OrderStatus.Shipped)
        {
            order.Process();
            order.Confirm();
            order.Ship();
        }

        return order;
    }

    [Fact]
    [Trait("Clip", "5")]
    public void OrdersByStatusSpec_ReturnsOnlyOrdersWithThatStatus()
    {
        var placed    = CreateOrder(OrderStatus.Placed);
        var cancelled = CreateOrder(OrderStatus.Cancelled);
        var shipped   = CreateOrder(OrderStatus.Shipped);
        var orders    = new List<Order> { placed, cancelled, shipped };

        var spec   = new OrdersByStatusSpecification(OrderStatus.Shipped);
        var result = spec.Evaluate(orders).ToList();

        result.ShouldHaveSingleItem();
        result[0].Status.ShouldBe(OrderStatus.Shipped);
    }

    [Fact]
    [Trait("Clip", "5")]
    public void MultiLineOrdersSpec_ReturnsOnlyOrdersWithMoreThanOneLine()
    {
        var singleLine = CreateOrder(OrderStatus.Placed, lineCount: 1);
        var twoLines   = CreateOrder(OrderStatus.Placed, lineCount: 2);
        var threeLines = CreateOrder(OrderStatus.Placed, lineCount: 3);
        var orders     = new List<Order> { singleLine, twoLines, threeLines };

        var spec   = new MultiLineOrdersSpecification();
        var result = spec.Evaluate(orders).ToList();

        result.Count.ShouldBe(2);
        result.ShouldAllBe(o => o.Lines.Count > 1);
    }

    [Fact]
    [Trait("Clip", "6")]
    public void OrderSearchSpec_WithStatusFilter_FiltersCorrectly()
    {
        var placedOrder  = CreateOrder(OrderStatus.Placed);
        var shippedOrder = CreateOrder(OrderStatus.Shipped);
        var allOrders    = new List<Order> { placedOrder, shippedOrder };

        var orderSearchSpecShippedStatus = new OrderSearchSpecification(OrderStatus.Shipped, null);
        var shippedOrders                = orderSearchSpecShippedStatus.Evaluate(allOrders).ToList();

        shippedOrders.ShouldHaveSingleItem();
        shippedOrders[0].Status.ShouldBe(OrderStatus.Shipped);
    }

    [Fact]
    [Trait("Clip", "6")]
    public void OrderSearchSpec_WithNoFilters_ReturnsAll()
    {
        var placedOrder  = CreateOrder(OrderStatus.Placed);
        var shippedOrder = CreateOrder(OrderStatus.Shipped);
        var allOrders    = new List<Order> { placedOrder, shippedOrder };

        var orderSearchSpecNoFilters = new OrderSearchSpecification(null, null);
        var unfilteredOrders         = orderSearchSpecNoFilters.Evaluate(allOrders).ToList();

        unfilteredOrders.Count.ShouldBe(2);
    }

    //TODO: Module 6 Clip 7 — Uncomment the LargeOrdersSpecification test below:
    //[Fact]
    //[Trait("Clip", "7")]
    //public void LargeOrdersSpec_ReturnsOnlyOrdersAtOrAboveTheMinimum()
    //{
    //    // CreateOrder gives every line a quantity of 1 at 10.00, so the total is 10.00 per line.
    //    var smallOrder = CreateOrder(OrderStatus.Placed, lineCount: 1);   // total 10.00
    //    var largeOrder = CreateOrder(OrderStatus.Placed, lineCount: 3);   // total 30.00
    //    var allOrders  = new List<Order> { smallOrder, largeOrder };
    //
    //    var largeOrdersSpec = new LargeOrdersSpecification(25m);
    //    var largeOrders     = largeOrdersSpec.Evaluate(allOrders).ToList();
    //
    //    largeOrders.ShouldHaveSingleItem();
    //    largeOrders[0].Total.Amount.ShouldBe(30m);
    //}

    //TODO: Module 6 Clip 7 — Uncomment the shadow-property boundary test below:
    //[Fact]
    //[Trait("Clip", "7")]
    //public void OrdersByCustomerSpec_CannotBeEvaluatedInMemory()
    //{
    //    var allOrders            = new List<Order> { CreateOrder(OrderStatus.Placed) };
    //    var ordersByCustomerSpec = new OrdersByCustomerSpecification(1);
    //
    //    var evaluationException = Should.Throw<InvalidOperationException>(
    //        () => ordersByCustomerSpec.Evaluate(allOrders).ToList());
    //    evaluationException.Message.ShouldContain("may only be used within Entity Framework LINQ queries");
    //}
}
