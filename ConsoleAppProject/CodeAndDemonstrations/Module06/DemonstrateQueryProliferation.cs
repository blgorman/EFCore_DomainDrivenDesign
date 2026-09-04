using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateQueryProliferation
{
    public static async Task ShowProliferationAsync(IServiceProvider serviceProvider)
    {
        // Seed reference data
        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(seedCtx);
        }

        // ── Step 1: Run the proliferating queries against live data ───────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Clip 1 — Running Proliferating Queries Against Live Data"),
            ConsoleColor.DarkBlue);

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderingContext>();

        // These are the scattered ad-hoc LINQ calls that would appear across service code
        var allPlaced = await context.Orders
            .Where(o => o.Status == OrderStatus.Placed)
            .ToListAsync();

        var openOrders = await context.Orders
            .Where(o => o.Status != OrderStatus.Shipped && o.Status != OrderStatus.Cancelled)
            .ToListAsync();

        var openWithLines = await context.Orders
            .Where(o => o.Status != OrderStatus.Shipped
                     && o.Status != OrderStatus.Cancelled
                     && o.Lines.Count > 1)
            .ToListAsync();

        var shipped = await context.Orders
            .Where(o => o.Status == OrderStatus.Shipped)
            .ToListAsync();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Ad-Hoc LINQ Results — Each Query Written Independently",
            new[]
            {
                "Query 1: context.Orders.Where(o => o.Status == OrderStatus.Placed)",
                $"         Result: {allPlaced.Count} order(s)",
                "",
                "Query 2: context.Orders.Where(o => o.Status != Shipped && o.Status != Cancelled)",
                $"         Result: {openOrders.Count} order(s)",
                "",
                "Query 3: context.Orders.Where(o => o.Status != Shipped",
                "                              && o.Status != Cancelled && o.Lines.Count > 1)",
                $"         Result: {openWithLines.Count} order(s)",
                "",
                "Query 4: context.Orders.Where(o => o.Status == OrderStatus.Shipped)",
                $"         Result: {shipped.Count} order(s)",
                "",
                "All four queries ran — but each is a one-off LINQ expression in service code.",
                "No reuse. No single place to change the rule. No domain language."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Step 2: The solution ───────────────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 2 — Solution: Encapsulate Every Query in a Specification"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Specification Pattern — Answering the Three Problems",
            new[]
            {
                "1. Method explosion         one class per rule, not a method per combination",
                "2. Rules outside the domain the class carries the name of the business rule",
                "3. Predicates you cannot    the predicate is a class a test can target",
                "   test alone               on its own",
                "",
                "Instead of context.Orders.Where(o => EF.Property<int>(o, \"CustomerId\") == id):",
                "  var spec   = new OrdersByCustomerSpecification(customerId);",
                "  var orders = await repository.ListAsync(spec);",
                "",
                "The caller never writes a WHERE clause.",
                "The query has a name, and the name is the business rule."
            }
        ));

        await Task.CompletedTask;
    }
}
