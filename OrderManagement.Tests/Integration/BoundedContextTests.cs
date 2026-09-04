using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.ReadModels;

namespace OrderManagement.Tests.Integration;

[Trait("Category", "Integration")]
[Trait("Module", "3")]
[Collection("MsSql")]
public class BoundedContextTests : IAsyncLifetime
{
    private readonly MsSqlFixture _fixture;
    private OrderingContext _orderingCtx = null!;
    private ShippingContext _shippingCtx = null!;
    private Customer _customer = null!;

    public BoundedContextTests(MsSqlFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        var orderingOptions = new DbContextOptionsBuilder<OrderingContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;
        _orderingCtx = new OrderingContext(orderingOptions);

        var shippingOptions = new DbContextOptionsBuilder<ShippingContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;
        _shippingCtx = new ShippingContext(shippingOptions);

        _customer = Customer.Create("Test Customer", $"test-{Guid.NewGuid()}@example.com");
        _orderingCtx.Customers.Add(_customer);
        await _orderingCtx.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        if (_customer.Id > 0)
        {
            // Cascade delete on the FK removes any orders owned by this customer
            _orderingCtx.Customers.Remove(_customer);
            await _orderingCtx.SaveChangesAsync();
        }
        await _orderingCtx.DisposeAsync();
        await _shippingCtx.DisposeAsync();
    }

    [Fact]
    [Trait("Clip", "5")]
    public async Task ShipmentReadModel_ReadsOrderPlacedViaOrderingContext()
    {
        var order = Order.Place(
            _customer.Id,
            new[] { (2001, 1, Money.Create(20m, "USD")) });
        _orderingCtx.Orders.Add(order);
        _orderingCtx.Entry(order).Property<int>("CustomerId").CurrentValue = _customer.Id;
        await _orderingCtx.SaveChangesAsync();

        var shipment = await _shippingCtx.Set<ShipmentReadModel>()
            .FirstOrDefaultAsync(s => s.Id == order.Id);

        shipment.ShouldNotBeNull();
        shipment.Id.ShouldBe(order.Id);
        shipment.Status.ShouldBe(OrderStatus.Placed);
        shipment.PlacedAt.ShouldBe(order.PlacedAt, TimeSpan.FromSeconds(1));
    }
}
