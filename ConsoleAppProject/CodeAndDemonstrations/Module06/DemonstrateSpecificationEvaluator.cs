using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Specifications;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateSpecificationEvaluator
{
    public static async Task ShowEvaluatorAsync(IServiceProvider serviceProvider)
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

        // ── Step 1: The one query method on the repository ────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Clip 5 — OrderRepository.ListAsync Holds No Query"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderRepository.ListAsync — the entire method",
            new[]
            {
                "public async Task<IReadOnlyList<Order>> ListAsync(",
                "    ISpecification<Order> spec, CancellationToken ct = default)",
                "{",
                "    var query = SpecificationEvaluator.Default",
                "        .GetQuery(_ctx.Orders.AsQueryable(), spec);",
                "",
                "    return await query.ToListAsync(ct);",
                "}",
                "",
                "No Where. No Include. No OrderBy. No status, no customer, no date.",
                "SpecificationEvaluator reads the expressions off whichever spec arrives",
                "and composes the IQueryable. ListAsync only executes the result."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Step 2: Four specifications, one unchanged method ─────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 2 — Four Different Queries Through That Same Method"),
            ConsoleColor.DarkBlue);

        //TODO: Module 6 Clip 5 — remove this block and uncomment the four-spec block below
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This screen unlocks in Module 6 Clip 5 — remove this block and uncomment the block below.",
                "Prerequisite: the Query.Where predicate in OrdersByStatusSpecification."
            }
        ));

        //var specs = new (string Name, ISpecification<Order> Spec)[]
        //{
        //    ("OrdersByCustomerSpecification(customerId)", new OrdersByCustomerSpecification(SeedDataHelper.CustomerAId)),
        //    ("OrdersByStatusSpecification(Shipped)",      new OrdersByStatusSpecification(OrderStatus.Shipped)),
        //    ("MultiLineOrdersSpecification()",            new MultiLineOrdersSpecification()),
        //    ("LargeOrdersSpecification(100m)",            new LargeOrdersSpecification(100m))
        //};
        //
        //var rows = new List<string>();
        //
        //foreach (var (name, spec) in specs)
        //{
        //    var results = await repository.ListAsync(spec);
        //    var ids     = string.Join(", ", results.Select(o => o.Id));
        //
        //    rows.Add($"repository.ListAsync(new {name})");
        //    rows.Add($"    {results.Count} order(s):  {ids}");
        //    rows.Add("");
        //}
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "Four Specifications, One Repository Method",
        //    rows
        //    .Concat(new[]
        //    {
        //        "---",
        //        "Four different queries just ran against the database.",
        //        "OrderRepository gained no method, and not one line of it changed.",
        //        "A fifth query is a fifth specification class, still no repository edit."
        //    })
        //    .ToArray()
        //));

        await Task.CompletedTask;
    }
}
