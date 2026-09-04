using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class SeedDataHelper
{
    // Resolved after SeedAsync runs — populated from the DB, not hardcoded.
    public static int CustomerAId { get; private set; }
    public static int CustomerBId { get; private set; }

    public static readonly int Product1Id = 2001;
    public static readonly int Product2Id = 2002;
    public static readonly int Product3Id = 2003;

    public static async Task SeedAsync(OrderingContext context)
    {
        await SeedCustomersAsync(context);
        await SeedOrdersAsync(context);
    }

    public static async Task EnsureCustomersAsync(OrderingContext context)
    {
        await SeedCustomersAsync(context);
    }

    private static async Task SeedCustomersAsync(OrderingContext context)
    {
        var alice = await context.Customers
            .FirstOrDefaultAsync(c => c.Email == "alice@example.com");
        if (alice is null)
        {
            alice = Customer.Create("Alice Andersen", "alice@example.com");
            context.Customers.Add(alice);
            await context.SaveChangesAsync();
        }
        CustomerAId = alice.Id;

        var bob = await context.Customers
            .FirstOrDefaultAsync(c => c.Email == "bob@example.com");
        if (bob is null)
        {
            bob = Customer.Create("Bob Barker", "bob@example.com");
            context.Customers.Add(bob);
            await context.SaveChangesAsync();
        }
        CustomerBId = bob.Id;
    }

    // Seeds directly through OrderingContext rather than IOrderRepository — the repository
    // is a stub until Module 4 Clip 4 is implemented, so its Add/SaveAsync are no-ops.
    // Once implemented, OrderRepository.SaveAsync is just a pass-through to
    // _ctx.SaveChangesAsync(), so this produces identical behavior either way, including
    // firing the DomainEventDispatchInterceptor (attached to OrderingContext, not the repo).
    private static async Task SeedOrdersAsync(OrderingContext context)
    {
        if (await context.Orders.AnyAsync())
            return;

        // Order 1: Placed, Customer A, 2 lines
        var order1 = Order.Place(CustomerAId, new[]
        {
            (Product1Id, 2, Money.Create(29.99m, "USD")),
            (Product2Id, 1, Money.Create(49.99m, "USD"))
        });
        context.Orders.Add(order1);
        //TODO: Module 2 Clip 5 — After making CustomerId a shadow property, set it here before saving:
        //context.Entry(order1).Property<int>("CustomerId").CurrentValue = CustomerAId;

        // Order 2: Placed, Customer A, 1 line
        var order2 = Order.Place(CustomerAId, new[]
        {
            (Product3Id, 3, Money.Create(9.99m, "USD"))
        });
        context.Orders.Add(order2);
        //TODO: Module 2 Clip 5 — After making CustomerId a shadow property, set it here before saving:
        //context.Entry(order2).Property<int>("CustomerId").CurrentValue = CustomerAId;

        //Order 3: Shipped, Customer B, 3 lines (ship after save below)
        var order3 = Order.Place(CustomerBId, new[]
        {
            (Product1Id, 1, Money.Create(29.99m, "USD")),
            (Product2Id, 2, Money.Create(49.99m, "USD")),
            (Product3Id, 4, Money.Create(9.99m, "USD"))
        });
        context.Orders.Add(order3);
        //TODO: Module 2 Clip 5 — After making CustomerId a shadow property, set it here before saving:
        //context.Entry(order3).Property<int>("CustomerId").CurrentValue = CustomerBId;

        //Order 4: Cancelled, Customer B, 2 lines (cancel after save below)
        var order4 = Order.Place(CustomerBId, new[]
        {
            (Product2Id, 1, Money.Create(49.99m, "USD")),
            (Product3Id, 2, Money.Create(9.99m, "USD"))
        });
        context.Orders.Add(order4);
        //TODO: Module 2 Clip 5 — After making CustomerId a shadow property, set it here before saving:
        //context.Entry(order4).Property<int>("CustomerId").CurrentValue = CustomerBId;

        // Orders 5–8: varied for Module 6 search demo
        var order5 = Order.Place(CustomerAId, new[]
        {
            (Product1Id, 5, Money.Create(29.99m, "USD"))
        });
        context.Orders.Add(order5);
        //TODO: Module 2 Clip 5 — After making CustomerId a shadow property, set it here before saving:
        //context.Entry(order5).Property<int>("CustomerId").CurrentValue = CustomerAId;

        var order6 = Order.Place(CustomerBId, new[]
        {
            (Product2Id, 3, Money.Create(49.99m, "USD"))
        });
        context.Orders.Add(order6);
        //TODO: Module 2 Clip 5 — After making CustomerId a shadow property, set it here before saving:
        //context.Entry(order6).Property<int>("CustomerId").CurrentValue = CustomerBId;

        var order7 = Order.Place(CustomerAId, new[]
        {
            (Product3Id, 10, Money.Create(9.99m, "USD"))
        });
        context.Orders.Add(order7);
        //TODO: Module 2 Clip 5 — After making CustomerId a shadow property, set it here before saving:
        //context.Entry(order7).Property<int>("CustomerId").CurrentValue = CustomerAId;

        var order8 = Order.Place(CustomerBId, new[]
        {
            (Product1Id, 2, Money.Create(29.99m, "USD")),
            (Product3Id, 3, Money.Create(9.99m, "USD"))
        });
        context.Orders.Add(order8);
        //TODO: Module 2 Clip 5 — After making CustomerId a shadow property, set it here before saving:
        //context.Entry(order8).Property<int>("CustomerId").CurrentValue = CustomerBId;

        // Save all as Placed first — the interceptor fires and dispatches OrderPlacedEvent to registered handlers
        await context.SaveChangesAsync();

        // Transition statuses using already-tracked instances — no need to re-query
        order3.Process();
        order3.Confirm();
        order3.Ship();
        order4.Cancel();
        await context.SaveChangesAsync();
    }

    // Ensures at least one order with lines exists in the DB.
    // Used by demos in early modules that need a real order to query, bypassing the stub OrderRepository.
    public static async Task<Order> EnsureOrderWithLinesAsync(OrderingContext ctx)
    {
        await SeedCustomersAsync(ctx);
        var existing = await ctx.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Lines.Count > 0);

        if (existing is not null)
            return existing;

        var order = Order.Place(
            CustomerAId,
            new[] { (Product1Id, 1, Money.Create(29.99m, "USD")) });

        ctx.Orders.Add(order);
        //TODO: Module 2 Clip 5 — After making CustomerId a shadow property, set it here before saving:
        //ctx.Entry(order).Property<int>("CustomerId").CurrentValue = CustomerAId;
        await ctx.SaveChangesAsync();

        return order;
    }
}
