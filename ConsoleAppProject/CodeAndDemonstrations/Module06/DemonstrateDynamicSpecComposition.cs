using ConsoleHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Specifications;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateDynamicSpecComposition
{
    public static async Task ShowCompositionAsync(IServiceProvider serviceProvider)
    {
        // Seed reference data
        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(seedCtx);
        }

        using var scope = serviceProvider.CreateScope();
        var repository  = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        // ── Step 1: No filters (every order) ──────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Clip 6 — No Filters: Every Order"),
            ConsoleColor.DarkBlue);

        var specOrderSearchNoFilters = new OrderSearchSpecification(null, null);
        var allOrders                = await repository.ListAsync(specOrderSearchNoFilters);


        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderSearchSpecification(null, null) — No Filters Active",
            new[]
            {
                "status:   null  — no Status WHERE clause added",
                "minLines: null  — no line count WHERE clause added",
                "",
                $"Orders returned: {allOrders.Count}  (all orders in database)",
                "",
                "The spec still ran through the evaluator — it just had no Where clauses.",
                "No branching in the repository. No special-case code for the empty filter."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Step 2: Status filter only ────────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 2 — Status Filter Only (Placed)"),
            ConsoleColor.DarkBlue);

        var specOrderSearchPlacedStatus = new OrderSearchSpecification(OrderStatus.Placed, null);
        var placedOrders                = await repository.ListAsync(specOrderSearchPlacedStatus);


        var placedOrderRows = placedOrders.Select(o =>
            $"    Id={o.Id}  Status={o.Status,-10}  PlacedAt={o.PlacedAt:yyyy-MM-dd}")
            .ToArray();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            $"OrderSearchSpecification(Placed, null) — {placedOrders.Count} result(s)",
            new[]
            {
                "status:   Placed  — WHERE o.Status == Placed added",
                "minLines: null    — no line count filter",
                "---"
            }
            .Concat(placedOrderRows)
            .Concat(new[]
            {
                "---",
                "Only one .Where() clause was added — the minLines clause was skipped."
            })
            .ToArray()
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Step 3: Both filters ──────────────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 3 — Both Filters (Placed + At Least 2 Lines)"),
            ConsoleColor.DarkBlue);

        var specOrderSearchPlacedWithMultipleLines = new OrderSearchSpecification(OrderStatus.Placed, 2);
        var placedMultiLineOrders                  = await repository.ListAsync(specOrderSearchPlacedWithMultipleLines);


        var placedMultiLineOrderRows = placedMultiLineOrders.Select(o =>
            $"    Id={o.Id}  Status={o.Status,-10}  Lines={o.Lines.Count}  PlacedAt={o.PlacedAt:yyyy-MM-dd}")
            .ToArray();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            $"OrderSearchSpecification(Placed, 2) — {placedMultiLineOrders.Count} result(s)",
            new[]
            {
                "status:   Placed  — WHERE o.Status == Placed",
                "minLines: 2       — AND o.Lines.Count >= 2",
                "---"
            }
            .Concat(placedMultiLineOrderRows)
            .Concat(new[]
            {
                "---",
                "Both .Where() clauses were added — the evaluator ANDed them together.",
                "One spec class handled all three scenarios with no repository changes."
            })
            .ToArray()
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Step 4: Why dynamic composition matters ────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 4 — Why Dynamic Composition Matters"),
            ConsoleColor.DarkBlue);


        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Dynamic Composition — Key Benefits",
            new[]
            {
                $"No filters:               {allOrders.Count} orders — no WHERE added",
                $"Status only:              {placedOrders.Count} orders — 1 WHERE added",
                $"Status + min lines:       {placedMultiLineOrders.Count} orders — 2 WHEREs added",
                "",
                "One specification class handled all three combinations.",
                "repository.ListAsync(spec) is the same call in every scenario."
            }
        ));

        await Task.CompletedTask;
    }
}
