using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateReadOnlyCollections
{
    public static async Task ShowCollectionAndShadowPropsAsync(IServiceProvider serviceProvider)
    {


        using var scope = serviceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        await SeedDataHelper.EnsureCustomersAsync(ctx);

        var order = Order.Place(
            SeedDataHelper.CustomerAId,
            new[] { (SeedDataHelper.Product1Id, 1, Money.Create(29.99m, "USD")) });
        ctx.Orders.Add(order);
        var customerIdAfterPlace = ctx.Entry(order).Property<int>("CustomerId").CurrentValue;

        ctx.Entry(order).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
        await ctx.SaveChangesAsync();

        using var readScope = serviceProvider.CreateScope();
        var readCtx = readScope.ServiceProvider.GetRequiredService<OrderingContext>();
        var reloaded = await readCtx.Orders.FirstAsync(o => o.Id == order.Id);
        var storedCustomerId = readCtx.Entry(reloaded).Property<int>("CustomerId").CurrentValue;

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Shadow Property — Order CustomerId"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "ctx.Entry(order).Property<int>(\"CustomerId\").CurrentValue",
            new[]
            {
                $"Customer Id passed to Order.Place:        {SeedDataHelper.CustomerAId}",
                $"Shadow CustomerId after Order.Place:      {customerIdAfterPlace}",
                $"Stored CustomerId after save and reload:  {storedCustomerId}",
                "",
                "Order.Place does not store the customer Id. Order has no CustomerId member.",
                "The demo sets the shadow CustomerId through the change tracker before saving.",
                "That line is contrived for demo purposes and is NOT production-ready code.",
                "When we implement the repository, that line moves into OrderRepository.Add,",
                "and application code never touches the change tracker."
            }
        ));

        await Task.CompletedTask;
    }
}
