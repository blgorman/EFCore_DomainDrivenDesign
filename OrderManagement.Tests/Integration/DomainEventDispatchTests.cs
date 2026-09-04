using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Events;
using OrderManagement.Infrastructure.Repositories;

namespace OrderManagement.Tests.Integration;

[Trait("Category", "Integration")]
[Trait("Module", "5")]
[Collection("MsSql")]
public class DomainEventDispatchTests : IAsyncLifetime
{
    private readonly MsSqlFixture _fixture;
    private IServiceProvider _serviceProvider = null!;
    private SpyOrderPlacedEventHandler _spy = null!;
    private readonly List<Order> _insertedOrders = new();
    private Customer _customer = null!;

    public DomainEventDispatchTests(MsSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public class SpyOrderPlacedEventHandler : IDomainEventHandler<OrderPlacedEvent>
    {
        public int CallCount { get; private set; }

        public Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken ct = default)
        {
            CallCount++;
            return Task.CompletedTask;
        }

        Task IDomainEventHandler.HandleAsync(IDomainEvent domainEvent, CancellationToken ct)
            => HandleAsync((OrderPlacedEvent)domainEvent, ct);
    }

    public async Task InitializeAsync()
    {
        _spy = new SpyOrderPlacedEventHandler();

        var services = new ServiceCollection();

        services.AddScoped<IDomainEventHandler<OrderPlacedEvent>>(_ => _spy);
        services.AddScoped<DomainEventDispatcher>();
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddDbContext<OrderingContext>((sp, options) =>
            options.UseSqlServer(_fixture.ConnectionString)
                   .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IOrderRepository, OrderRepository>();

        _serviceProvider = services.BuildServiceProvider();

        using var seedScope = _serviceProvider.CreateScope();
        var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
        _customer = Customer.Create("Event Test Customer", $"events-{Guid.NewGuid()}@example.com");
        seedCtx.Customers.Add(_customer);
        await seedCtx.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        if (_insertedOrders.Count > 0)
        {
            using var scope = _serviceProvider.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();
            var ids = _insertedOrders.Select(o => o.Id).Where(id => id > 0).ToList();
            if (ids.Count > 0)
            {
                var orders = await ctx.Orders.Where(o => ids.Contains(o.Id)).ToListAsync();
                ctx.Orders.RemoveRange(orders);
                await ctx.SaveChangesAsync();
            }
        }

        if (_customer.Id > 0)
        {
            using var customerScope = _serviceProvider.CreateScope();
            var customerCtx = customerScope.ServiceProvider.GetRequiredService<OrderingContext>();
            customerCtx.Customers.Remove(customerCtx.Customers.Find(_customer.Id)!);
            await customerCtx.SaveChangesAsync();
        }

        if (_serviceProvider is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
    }

    private Order CreateTrackedOrder()
    {
        var order = Order.Place(
            _customer.Id,
            new[] { (Random.Shared.Next(1, int.MaxValue), 1, Money.Create(25.00m, "USD")) });
        _insertedOrders.Add(order);
        return order;
    }

    [Fact]
    [Trait("Clip", "6")]
    public async Task PlaceOrder_SaveAsync_DispatchesOrderPlacedEvent_Once()
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        var order = CreateTrackedOrder();
        repo.Add(order, _customer.Id);
        await repo.SaveAsync();

        _spy.CallCount.ShouldBe(1);
    }

    [Fact]
    [Trait("Clip", "6")]
    public async Task PlaceOrder_SaveAsync_ClearsEventsAfterDispatch()
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        var order = CreateTrackedOrder();
        repo.Add(order, _customer.Id);
        await repo.SaveAsync();

        order.DomainEvents.ShouldBeEmpty();
    }
}

