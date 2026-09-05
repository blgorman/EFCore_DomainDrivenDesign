using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateBackingFields
{
    public static async Task ShowBackingFieldConfigAsync(IServiceProvider serviceProvider)
    {


        using var scope = serviceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();

        var order = await SeedDataHelper.EnsureOrderWithLinesAsync(ctx);

        order = await ctx.Orders
            .Include(o => o.Lines)
            .FirstAsync(o => o.Id == order.Id);

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Backing Field Configuration"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Backing Field Configuration — Order._lines",
            new[]
            {
                "Order.Lines is backed by the private field '_lines'.",
                "EF config: builder.Navigation(o => o.Lines).HasField(\"_lines\")",
                "  -> tells EF to read/write '_lines' directly, bypassing the public property.",
                "The public property 'Lines' exposes IReadOnlyCollection<OrderLine>.",
                "Callers can never mutate the collection — only domain methods can (via AddLine)."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // These lines are intentional compile errors — do NOT uncomment them:
        // order.Lines.Add(new OrderLine(Random.Shared.Next(1, int.MaxValue), 2, Money.Create(19.99m, "USD")));  // CS1061: no Add on IReadOnlyCollection
        // var line = new OrderLine(Random.Shared.Next(1, int.MaxValue), 1, new Money(10m, "USD"));              // CS0122: internal constructor inaccessible

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("What Doesn't Compile"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Compile-Time Enforcement — Why Those Lines Are Commented Out",
            new[]
            {
                "order.Lines.Add(...)       CS1061 — IReadOnlyCollection has no Add/Remove/Clear.",
                "new OrderLine(...)         CS0122 — internal constructor, only Order.AddLine() can call it.",
                "",
                "The only legal path: order.AddLine(productId, qty, price)"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // THIS is the only legal way to add a line — through the domain method:
        var lineCountBefore = order.Lines.Count;
        order.AddLine(SeedDataHelper.Product3Id, 2, Money.Create(19.99m, "USD"));
        var lineCountAfter = order.Lines.Count;

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("AddLine — The Only Legal Path"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "order.AddLine(...) — Domain Method Enforces the Boundary",
            new[]
            {
                "order.AddLine(SeedDataHelper.Product3Id, 2, Money.Create(19.99m, \"USD\"))",
                "",
                $"Lines count BEFORE AddLine: {lineCountBefore}",
                $"Lines count AFTER  AddLine: {lineCountAfter}",
                "",
                "AddLine writes directly to the private _lines field.",
                "EF (via HasField) also uses _lines — the public property is never written to.",
                "The domain model stays in control of its own collection at all times."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Reloaded Order Values"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Reloaded Order Values: _lines Field",
            new[]
            {
                $"Order Id:   {order.Id}",
                $"CustomerId: {order.CustomerId}",
                //TODO: Module 2 Clip 5 — Replace the line above with this after making CustomerId a shadow property:
                // $"CustomerId: {ctx.Entry(order).Property<int>("CustomerId").CurrentValue}",
                $"Status:     {order.Status}",
                $"Reloaded order — Lines count from private _lines field: {order.Lines.Count}"
            }
        ));

        await Task.CompletedTask;
    }
}
