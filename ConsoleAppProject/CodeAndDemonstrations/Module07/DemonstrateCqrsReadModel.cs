using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.ReadModels;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateCqrsReadModel
{
    public static async Task ShowReadModelAsync(IServiceProvider serviceProvider)
    {
        // Seed reference data
        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(seedCtx);
        }

        using var scope = serviceProvider.CreateScope();
        var context    = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        // ── Screen 1: The two paths ───────────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Module 7 — CQRS: A Read Model Beside the Write Model"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Two repositories, two jobs",
            new[]
            {
                "Write path: IOrderRepository loads the full Order aggregate.",
                "    Change-tracked, every invariant enforceable, every line loaded.",
                "",
                "Read path: OrderReadRepository projects straight to OrderSummaryDto.",
                "    Select projects to columns — no aggregate, nothing tracked.",
                "",
                "The write side is unchanged from Module 4.",
                "The read side is new: 2 files, zero changes to existing code."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Screen 2: The write path loads the aggregate ──────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Write Path — the Full Aggregate"),
            ConsoleColor.DarkBlue);

        var firstOrderId = await context.Orders
            .OrderBy(o => o.Id)
            .Select(o => o.Id)
            .FirstAsync();

        var orderAggregate = (await repository.GetByIdAsync(firstOrderId))!;
        var trackedAfterWrite = context.ChangeTracker.Entries().Count();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            $"repository.GetByIdAsync({firstOrderId})",
            new[]
            {
                //TODO: Module 7 Clip 4 — Delete the line below and uncomment the //WITH: lines (requires having completed Module 2 Clip 4, which changes Total to Money):
                $"Order {orderAggregate.Id}: Status {orderAggregate.Status}, {orderAggregate.Lines.Count} line(s)",
                //WITH:
                //$"Order {orderAggregate.Id}: Status {orderAggregate.Status}, {orderAggregate.Lines.Count} line(s), " +
                //$"Total {orderAggregate.Total.Amount} {orderAggregate.Total.Currency}",
                $"ChangeTracker holds {trackedAfterWrite} entries: the Order and its {orderAggregate.Lines.Count} line(s).",
                "",
                "The aggregate is change-tracked. Process, Confirm, Ship, and Cancel",
                "can run against it, and SaveAsync persists the transitions.",
                "That power costs a JOIN to OrderLine on every load."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Screen 3: The read path projects to DTOs ──────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Read Path — OrderReadRepository.ListSummariesAsync"),
            ConsoleColor.DarkBlue);

        var readRepository    = new OrderReadRepository(context);
        var trackedBeforeRead = context.ChangeTracker.Entries().Count();
        var summaries         = await readRepository.ListSummariesAsync();
        var trackedAfterRead  = context.ChangeTracker.Entries().Count();

        var summaryRows = summaries
            .Select(s => $"Order {s.OrderId} | Customer {s.CustomerId} | {s.Status,-10} | " +
                         $"{s.LineCount} line(s) | {s.PlacedAt:yyyy-MM-dd HH:mm:ss}")
            .Concat(new[]
            {
                "",
                $"{summaries.Count} rows, newest first. No Order aggregate was materialized.",
                $"ChangeTracker went from {trackedBeforeRead} entries to {trackedAfterRead}: the read query added none.",
                "Select reads only these columns, and a projection is never tracked.",
                "IOrderRepository mutates, OrderReadRepository reads.",
                "That split between the write type and the read type is the first step toward CQRS."
            })
            .ToArray();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Every order as a flat OrderSummaryDto",
            summaryRows
        ));

        await Task.CompletedTask;
    }
}
