using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateOwnedEntities
{
    public static async Task ShowMoneyMappingAsync(IServiceProvider serviceProvider)
    {
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Owned Entity (Money) Mapping — Order.Total"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Starter State — Order.Total is a plain decimal",
            new[]
            {
                "Order.Total is currently typed as decimal.",
                "A decimal carries no currency context — it is just a number.",
                "Nothing in the type prevents mixing USD and EUR totals accidentally.",
                "",
                "The Orders table has a single 'Total' column (decimal).",
                "Clip 4 adds a 'Currency' column and wraps both into a Money value object.",
                "The 'Total' column name stays the same — only the mapping changes.",
            }
        ));

        //-------------------------------------------------------
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //-------------------------------------------------------

        using var scope = serviceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        await SeedDataHelper.EnsureCustomersAsync(ctx);

        var order = Order.Place(
            SeedDataHelper.CustomerAId,
            new[]
            {
                (SeedDataHelper.Product1Id, 2, Money.Create(29.99m, "USD")),
                (SeedDataHelper.Product2Id, 1, Money.Create(149.99m, "USD"))
            }
        );

        ctx.Orders.Add(order);
        ctx.Entry(order).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
        await ctx.SaveChangesAsync();

        var savedId = order.Id;

        using var scope2 = serviceProvider.CreateScope();
        var ctx2 = scope2.ServiceProvider.GetRequiredService<OrderingContext>();
        var reloaded = await ctx2.Orders.FirstOrDefaultAsync(o => o.Id == savedId);

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Order.Total — After Clip 4 (Money with OwnsOne)"), ConsoleColor.DarkBlue);
        
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Order saved and reloaded — Total as Money (OwnsOne)",
            new[]
            {
                $"Order Id:    {reloaded!.Id}",
                $"Total.Amount:    {reloaded.Total.Amount:F2}",
                $"Total.Currency:  {reloaded.Total.Currency}",
                "",
                "After updating to Money, both columns are stored in the Orders table — no join, no separate table.",
                "The 'Total' column holds the amount; 'Currency' holds the currency.",
                "Only the schema changed, adding one column (Currency). Total column unchanged."
            }
        ));

    }
}
