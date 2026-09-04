using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Repositories;
using OrderManagement.Infrastructure.Specifications;

namespace OrderManagement.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("MsSql")]
public class OrderRepositoryTests : IAsyncLifetime
{
    private readonly MsSqlFixture _fixture;
    private OrderingContext _ctx = null!;
    private OrderRepository _repo = null!;
    private int _customerAId;
    private int _customerBId;
    private readonly List<Order> _insertedOrders = new();

    public OrderRepositoryTests(MsSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        // Note: OrderingContext is created without the DomainEventDispatchInterceptor.
        // These persistence tests intentionally cover repository behaviour only, not event dispatch.
        var options = new DbContextOptionsBuilder<OrderingContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;
        _ctx = new OrderingContext(options);
        _repo = new OrderRepository(_ctx);

        // Orders carry a shadow CustomerId FK (FK_Orders_Customers_CustomerId), so a real
        // Customer row must exist before any Order can be saved. Seed two by email (idempotent).
        _customerAId = await EnsureCustomerAsync("alice@example.com", "Alice Andersen");
        _customerBId = await EnsureCustomerAsync("bob@example.com", "Bob Barker");
    }

    private async Task<int> EnsureCustomerAsync(string email, string name)
    {
        var existing = await _ctx.Customers.FirstOrDefaultAsync(c => c.Email == email);
        if (existing is not null)
            return existing.Id;

        var customer = Customer.Create(name, email);
        _ctx.Customers.Add(customer);
        await _ctx.SaveChangesAsync();
        return customer.Id;
    }

    public async Task DisposeAsync()
    {
        // Clean up every order inserted by this test so count-based assertions in other
        // tests are not affected by leftover rows.
        if (_insertedOrders.Count > 0)
        {
            var ids = _insertedOrders.Select(o => o.Id).Where(id => id > 0).ToList();
            if (ids.Count > 0)
            {
                var orders = await _ctx.Orders
                    .Where(o => ids.Contains(o.Id))
                    .ToListAsync();
                _ctx.Orders.RemoveRange(orders);
                await _ctx.SaveChangesAsync();
            }
        }

        await _ctx.DisposeAsync();
    }

    // Order.Place() does not set the CustomerId shadow FK. The Add(order, customerId)
    // overload sets it, against a real seeded customer.
    private Order CreateTestOrder(int customerId)
    {
        var order = Order.Place(
            customerId,
            new[] { (Random.Shared.Next(1, int.MaxValue), 2, Money.Create(15.00m, "USD")) });
        _repo.Add(order, customerId);
        _insertedOrders.Add(order);
        return order;
    }

    [Fact]
    [Trait("Module", "4")]
    [Trait("Clip", "4")]
    public async Task Add_ThenGetById_ReturnsOrderWithLines()
    {
        var order = CreateTestOrder(_customerAId);
        await _repo.SaveAsync();

        var loaded = await _repo.GetByIdAsync(order.Id);

        loaded.ShouldNotBeNull();
        loaded.Id.ShouldBe(order.Id);
        loaded.Lines.ShouldHaveSingleItem();
        loaded.Lines.First().UnitPrice.Amount.ShouldBe(15.00m);
        loaded.Lines.First().UnitPrice.Currency.ShouldBe("USD");
    }

    [Fact]
    [Trait("Module", "4")]
    [Trait("Clip", "4")]
    public async Task Remove_ThenGetById_ReturnsNull()
    {
        var order = CreateTestOrder(_customerAId);
        await _repo.SaveAsync();

        _repo.Remove(order);
        await _repo.SaveAsync();

        // Order was hard-deleted; remove from cleanup list so DisposeAsync doesn't try to delete it again.
        _insertedOrders.Remove(order);

        var loaded = await _repo.GetByIdAsync(order.Id);
        loaded.ShouldBeNull();
    }

    [Fact]
    [Trait("Module", "6")]
    [Trait("Clip", "2")]
    public async Task ListAsync_WithOrdersByCustomerSpec_ReturnsOnlyThatCustomersOrders()
    {
        var orderA1 = CreateTestOrder(_customerAId);
        var orderA2 = CreateTestOrder(_customerAId);
        var orderB  = CreateTestOrder(_customerBId);

        await _repo.SaveAsync();

        var spec   = new OrdersByCustomerSpecification(_customerAId);
        var result = await _repo.ListAsync(spec);

        var resultIds = result.Select(o => o.Id).ToList();
        resultIds.ShouldContain(orderA1.Id);
        resultIds.ShouldContain(orderA2.Id);
        resultIds.ShouldNotContain(orderB.Id);
    }
}
