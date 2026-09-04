using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace OrderManagement.Tests.Integration;

[Collection("MsSql")]
[Trait("Category", "Integration")]
[Trait("Module", "2")]
public class AggregateConfigurationTests : IAsyncLifetime
{
    private readonly MsSqlFixture _fixture;
    private OrderingContext _ctx = null!;
    private Customer _customer = null!;
    private readonly List<int> _insertedOrderIds = new();

    public AggregateConfigurationTests(MsSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<OrderingContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;
        _ctx = new OrderingContext(options);

        _customer = Customer.Create("Integration Test Customer", $"m2-{Guid.NewGuid()}@test.com");
        _ctx.Customers.Add(_customer);
        await _ctx.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        if (_insertedOrderIds.Count > 0)
        {
            var orders = await _ctx.Orders
                .Where(o => _insertedOrderIds.Contains(o.Id))
                .ToListAsync();
            _ctx.Orders.RemoveRange(orders);
            await _ctx.SaveChangesAsync();
        }

        _ctx.Customers.Remove(_customer);
        await _ctx.SaveChangesAsync();

        await _ctx.DisposeAsync();
    }

    [Fact]
    [Trait("Clip", "2")]
    public async Task Order_Lines_AreReadOnly_AfterRoundTrip()
    {
        var order = Order.Place(
            _customer.Id,
            new[] { (2001, 1, Money.Create(10m, "USD")) });

        _ctx.Orders.Add(order);
        _ctx.Entry(order).Property<int>("CustomerId").CurrentValue = _customer.Id;
        await _ctx.SaveChangesAsync();
        _insertedOrderIds.Add(order.Id);
        _ctx.ChangeTracker.Clear();

        var reloaded = await _ctx.Orders
            .Include(o => o.Lines)
            .FirstAsync(o => o.Id == order.Id);

        // Fails in starter state: List<T>.IsReadOnly = false.
        // Passes after Clip 2: Lines returns ReadOnlyCollection<T>.IsReadOnly = true.
        ((ICollection<OrderLine>)reloaded.Lines).IsReadOnly.ShouldBeTrue();
    }

    [Fact]
    [Trait("Clip", "2")]
    public async Task Order_Lines_LoadedFromDatabase_ViaHasFieldConfig()
    {
        var order = Order.Place(
            _customer.Id,
            new[] { (2001, 2, Money.Create(19.99m, "USD")) });

        _ctx.Orders.Add(order);
        _ctx.Entry(order).Property<int>("CustomerId").CurrentValue = _customer.Id;
        await _ctx.SaveChangesAsync();
        _insertedOrderIds.Add(order.Id);
        _ctx.ChangeTracker.Clear();

        var reloaded = await _ctx.Orders
            .Include(o => o.Lines)
            .FirstAsync(o => o.Id == order.Id);

        // EF Core resolves Lines to the _lines backing field by convention, with or without HasField("_lines") in OrderConfiguration
        reloaded.Lines.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Clip", "5")]
    public async Task Order_CustomerId_RoundTrips_ViaShadowProperty()
    {
        var order = Order.Place(
            _customer.Id,
            new[] { (2001, 1, Money.Create(10m, "USD")) });

        _ctx.Orders.Add(order);
        _ctx.Entry(order).Property<int>("CustomerId").CurrentValue = _customer.Id;
        await _ctx.SaveChangesAsync();
        _insertedOrderIds.Add(order.Id);
        _ctx.ChangeTracker.Clear();

        var reloaded = await _ctx.Orders.FirstAsync(o => o.Id == order.Id);

        _ctx.Entry(reloaded).Property<int>("CustomerId").CurrentValue
            .ShouldBe(_customer.Id);
    }
}
