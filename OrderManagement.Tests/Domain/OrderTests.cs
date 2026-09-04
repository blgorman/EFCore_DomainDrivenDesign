using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Tests.Domain;

[Trait("Category", "Unit")]
[Trait("Module", "2")]
public class OrderTests
{
    private static IEnumerable<(int productId, int quantity, Money unitPrice)> CreateLines(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            yield return (Random.Shared.Next(1, int.MaxValue), i + 1, Money.Create(10.00m * (i + 1), "USD"));
        }
    }

    [Fact]
    [Trait("Clip", "2")]
    public void Lines_IsReadOnly_CannotBeDirectlyMutated()
    {
        var order = Order.Place(Random.Shared.Next(1, int.MaxValue), CreateLines(1));

        ((ICollection<OrderLine>)order.Lines).IsReadOnly.ShouldBeTrue();
    }

    [Fact]
    [Trait("Clip", "2")]
    public void Place_CreatesOrderWithCorrectStatus()
    {
        var customerId = Random.Shared.Next(1, int.MaxValue);
        var order = Order.Place(customerId, CreateLines());

        order.Status.ShouldBe(OrderStatus.Placed);
    }

    [Fact]
    [Trait("Clip", "2")]
    public void Place_CreatesOrderWithLines()
    {
        var customerId = Random.Shared.Next(1, int.MaxValue);
        var order = Order.Place(customerId, CreateLines(3));

        order.Lines.Count.ShouldBe(3);
    }

    [Fact]
    [Trait("Clip", "2")]
    public void Place_RaisesDomainEvent_OrderPlacedEvent()
    {
        var customerId = Random.Shared.Next(1, int.MaxValue);
        var order = Order.Place(customerId, CreateLines());

        order.DomainEvents.ShouldHaveSingleItem();
        order.DomainEvents[0].ShouldBeOfType<OrderPlacedEvent>();
    }

    [Fact]
    [Trait("Clip", "3")]
    public void Order_Status_HasPrivateSetter()
    {
        typeof(Order).GetProperty("Status")!.SetMethod!.IsPrivate.ShouldBeTrue();
    }

    [Fact]
    [Trait("Clip", "3")]
    public void Order_Id_HasPrivateSetter()
    {
        typeof(Order).GetProperty("Id")!.SetMethod!.IsPrivate.ShouldBeTrue();
    }

    [Fact]
    [Trait("Clip", "3")]
    public void Order_PlacedAt_HasPrivateSetter()
    {
        typeof(Order).GetProperty("PlacedAt")!.SetMethod!.IsPrivate.ShouldBeTrue();
    }

    [Fact]
    [Trait("Clip", "3")]
    public void Order_Total_HasPrivateSetter()
    {
        typeof(Order).GetProperty("Total")!.SetMethod!.IsPrivate.ShouldBeTrue();
    }

    [Fact]
    [Trait("Clip", "4")]
    public void Order_Total_PropertyType_IsMoney()
    {
        typeof(Order).GetProperty("Total")!.PropertyType.ShouldBe(typeof(Money));
    }

    [Fact]
    [Trait("Clip", "4")]
    public void Place_Total_IsMoneyValueObject()
    {
        var order = Order.Place(1, new[] { (2001, 1, Money.Create(10.00m, "USD")) });

        // Cast via object so this compiles whether Total is decimal or Money —
        // ShouldBeOfType<Money> fails at runtime when Total is still a plain decimal.
        ((object)order.Total).ShouldBeOfType<Money>();
    }

    [Fact]
    [Trait("Clip", "4")]
    public void Place_Total_CurrencyMatchesFirstLineCurrency()
    {
        var order = Order.Place(1, new[] { (2001, 1, Money.Create(10.00m, "GBP")) });

        var money = ((object)order.Total).ShouldBeOfType<Money>();
        money.Currency.ShouldBe("GBP");
    }

    [Fact]
    [Trait("Clip", "3")]
    public void Cancel_SetsStatusCancelled()
    {
        var customerId = Random.Shared.Next(1, int.MaxValue);
        var order = Order.Place(customerId, CreateLines());

        order.Cancel();

        order.Status.ShouldBe(OrderStatus.Cancelled);
    }

    [Fact]
    [Trait("Clip", "8")]
    public void Cancel_AfterShipped_ThrowsInvalidOperationException()
    {
        var customerId = Random.Shared.Next(1, int.MaxValue);
        var order = Order.Place(customerId, CreateLines());
        order.Process();
        order.Confirm();
        order.Ship();

        Should.Throw<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    [Trait("Clip", "8")]
    public void Cancel_AlreadyCancelled_Throws()
    {
        var customerId = Random.Shared.Next(1, int.MaxValue);
        var order = Order.Place(customerId, CreateLines());
        order.Cancel();

        Should.Throw<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    [Trait("Clip", "3")]
    public void Ship_SetsStatusShipped()
    {
        var customerId = Random.Shared.Next(1, int.MaxValue);
        var order = Order.Place(customerId, CreateLines());
        order.Process();
        order.Confirm();

        order.Ship();

        order.Status.ShouldBe(OrderStatus.Shipped);
    }

    [Fact]
    [Trait("Clip", "3")]
    public void Ship_RaisesDomainEvent_OrderShippedEvent()
    {
        var customerId = Random.Shared.Next(1, int.MaxValue);
        var order = Order.Place(customerId, CreateLines());
        order.Process();
        order.Confirm();
        order.ClearEvents();

        order.Ship();

        order.DomainEvents.ShouldHaveSingleItem();
        order.DomainEvents[0].ShouldBeOfType<OrderShippedEvent>();
    }

    [Fact]
    [Trait("Clip", "3")]
    public void Cancel_RaisesDomainEvent_OrderCancelledEvent()
    {
        var customerId = Random.Shared.Next(1, int.MaxValue);
        var order = Order.Place(customerId, CreateLines());
        order.ClearEvents();

        order.Cancel();

        order.DomainEvents.ShouldHaveSingleItem();
        order.DomainEvents[0].ShouldBeOfType<OrderCancelledEvent>();
    }

    [Fact]
    [Trait("Clip", "3")]
    public void Process_SetsStatusProcessing()
    {
        var order = Order.Place(Random.Shared.Next(1, int.MaxValue), CreateLines());

        order.Process();

        order.Status.ShouldBe(OrderStatus.Processing);
    }

    [Fact]
    [Trait("Clip", "3")]
    public void Confirm_SetsStatusConfirmed()
    {
        var order = Order.Place(Random.Shared.Next(1, int.MaxValue), CreateLines());
        order.Process();

        order.Confirm();

        order.Status.ShouldBe(OrderStatus.Confirmed);
    }

    [Fact]
    [Trait("Clip", "8")]
    public void Place_WithZeroCustomerId_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Order.Place(0, CreateLines()));
    }

    [Fact]
    [Trait("Clip", "8")]
    public void Place_WithNullLines_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => Order.Place(1, null!));
    }

    [Fact]
    [Trait("Clip", "8")]
    public void Place_WithEmptyLines_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => Order.Place(1, Enumerable.Empty<(int, int, Money)>()));
    }

    [Fact]
    [Trait("Clip", "8")]
    public void AddLine_WithZeroProductId_ThrowsArgumentException()
    {
        var order = Order.Place(Random.Shared.Next(1, int.MaxValue), CreateLines());

        Should.Throw<ArgumentException>(() => order.AddLine(0, 1, Money.Create(10m, "USD")));
    }

    [Fact]
    [Trait("Clip", "8")]
    public void AddLine_DuplicateProduct_ThrowsInvalidOperationException()
    {
        const int productId = 42;
        var order = Order.Place(
            Random.Shared.Next(1, int.MaxValue),
            new[] { (productId, 1, Money.Create(10m, "USD")) });

        Should.Throw<InvalidOperationException>(() => order.AddLine(productId, 1, Money.Create(10m, "USD")));
    }

    [Fact]
    [Trait("Clip", "8")]
    public void AddLine_WithZeroQuantity_ThrowsArgumentException()
    {
        var order = Order.Place(Random.Shared.Next(1, int.MaxValue), CreateLines());

        Should.Throw<ArgumentException>(() => order.AddLine(99, 0, Money.Create(10m, "USD")));
    }
}
