using Ardalis.Specification.EntityFrameworkCore;
using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Specifications;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateSpecificationWithIncludes
{
    public static async Task ShowIncludesAsync(IServiceProvider serviceProvider)
    {
        // Seed reference data
        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(seedCtx);
        }

        using var scope = serviceProvider.CreateScope();
        var context     = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        var repository  = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        var spec = new OrdersByCustomerSpecification(SeedDataHelper.CustomerAId);

        // ── Step 1: Inspect the spec's Include and OrderBy declarations ───────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Clip 4 — Inspect OrdersByCustomerSpecification Includes and Order"),
            ConsoleColor.DarkBlue);

        //TODO: Module 6 Clip 4 — remove this block and uncomment the internals block below
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This screen unlocks in Module 6 Clip 4 — remove this block and uncomment the block below.",
                "Prerequisite: the Query.Include and OrderByDescending calls in OrdersByCustomerSpecification."
            }
        ));

        //var includeCount = spec.IncludeExpressions.Count();
        //var orderCount   = spec.OrderExpressions.Count();
        //
        //var firstInclude  = spec.IncludeExpressions.FirstOrDefault();
        //var includeDesc   = firstInclude?.LambdaExpression?.ToString() ?? "(none)";
        //
        //var firstOrder    = spec.OrderExpressions.FirstOrDefault();
        //var orderTypeDesc = firstOrder?.OrderType.ToString() ?? "(none)";
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "OrdersByCustomerSpecification — Include and Order Now Recorded",
        //    new[]
        //    {
        //        $"IncludeExpressions declared: {includeCount}",
        //        $"  Expression: {includeDesc}",
        //        "",
        //        $"OrderExpressions declared:   {orderCount}",
        //        $"  Order type: {orderTypeDesc}  (by PlacedAt)",
        //        "",
        //        "The same object already carried the Where predicate.",
        //        "Loading and ordering are now declared beside it, in one constructor."
        //    }
        //));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Step 2: Execute — Lines arrive loaded and the rows arrive ordered ─
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 2 — Execute: Lines Loaded and Rows Ordered by the Specification"),
            ConsoleColor.DarkBlue);

        //TODO: Module 6 Clip 4 — remove this block and uncomment the execute block below
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This screen unlocks in Module 6 Clip 4 — remove this block and uncomment the block below.",
                "Prerequisite: the Query.Include and OrderByDescending calls in OrdersByCustomerSpecification."
            }
        ));

        //var sql = SpecificationEvaluator.Default
        //    .GetQuery(context.Orders.AsQueryable(), spec)
        //    .ToQueryString();
        //
        //var orders = await repository.ListAsync(spec);
        //
        //var resultLines = orders
        //    .SelectMany(o => new[]
        //        {
        //            $"    Order {o.Id}   PlacedAt={o.PlacedAt:yyyy-MM-dd HH:mm}   Lines={o.Lines.Count}"
        //        }
        //        .Concat(o.Lines.Select(l =>
        //            $"        Product {l.ProductId}   Qty {l.Quantity}" +
        //            $"   @ {l.UnitPrice.Amount:0.00} {l.UnitPrice.Currency}" +
        //            $"   = {l.LineTotal.Amount:0.00} {l.LineTotal.Currency}")))
        //    .ToArray();
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    $"repository.ListAsync(spec) — {orders.Count} order(s), newest first",
        //    new[]
        //    {
        //        "SQL EF Core sent — note the JOIN and the ORDER BY:",
        //        "---"
        //    }
        //    .Concat(sql.Split(Environment.NewLine))
        //    .Concat(new[]
        //    {
        //        "---",
        //        "Rows returned:",
        //        "---"
        //    })
        //    .Concat(resultLines)
        //    .Concat(new[]
        //    {
        //        "---",
        //        $"Lines.Count > 0 on every order: {orders.All(o => o.Lines.Count > 0)}",
        //        "No demo code called LoadAsync. Query.Include did the loading.",
        //        "No demo code called OrderByDescending. The specification carried the order."
        //    })
        //    .ToArray()
        //));

        await Task.CompletedTask;
    }
}
