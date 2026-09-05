using Ardalis.Specification.EntityFrameworkCore;
using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Specifications;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateSpecificationBase
{
    public static async Task ShowBaseSpecAsync(IServiceProvider serviceProvider)
    {
        // Seed so spec can be run against live data
        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(seedCtx);
        }

        using var scope = serviceProvider.CreateScope();
        var context     = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        var repository  = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        var spec = new OrdersByCustomerSpecification(SeedDataHelper.CustomerAId);

        // ── Step 1: Create a spec and inspect its internals ───────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Clip 2 — Create OrdersByCustomerSpecification and Inspect Internals"),
            ConsoleColor.DarkBlue);


        var whereCount   = spec.WhereExpressions.Count();
        var includeCount = spec.IncludeExpressions.Count();
        var orderCount   = spec.OrderExpressions.Count();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrdersByCustomerSpecification — What the Constructor Recorded",
            new[]
            {
                $"WhereExpressions count:   {whereCount}",
                "  Predicate: EF.Property<int>(o, \"CustomerId\") == customerId",
                "",
                $"IncludeExpressions count: {includeCount}",
                $"OrderExpressions count:   {orderCount}",
                "",
                "Constructing the specification opened no connection and ran no SQL.",
                "Query.Where stored the predicate as an expression the object now carries."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Step 2: Hand the spec to the repository and run it ────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 2 — Run the Specification Through the Repository"),
            ConsoleColor.DarkBlue);


        var sql = SpecificationEvaluator.Default
            .GetQuery(context.Orders.AsQueryable(), spec)
            .ToQueryString();

        var orders = await repository.ListAsync(spec);

        var orderRows = orders
            .Select(o => $"    Id={o.Id}  Status={o.Status,-10}  PlacedAt={o.PlacedAt:yyyy-MM-dd}")
            .ToArray();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            $"repository.ListAsync(spec) — {orders.Count} order(s) for Customer {SeedDataHelper.CustomerAId}",
            new[]
            {
                "SQL EF Core sent:",
                "---"
            }
            .Concat(sql.Split(Environment.NewLine))
            .Concat(new[]
            {
                "---",
                "Rows returned:",
                "---"
            })
            .Concat(orderRows)
            .Concat(new[]
            {
                "---",
                "The WHERE clause came from Query.Where inside the specification.",
                "The calling code passed an object and wrote no LINQ of its own."
            })
            .ToArray()
        ));

        await Task.CompletedTask;
    }
}
