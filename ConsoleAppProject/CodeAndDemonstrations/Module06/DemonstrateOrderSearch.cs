using ConsoleHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Specifications;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateOrderSearch
{
    public static async Task RunEndToEndAsync(IServiceProvider serviceProvider)
    {
        // Seed reference data
        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(seedCtx);
        }

        using var scope = serviceProvider.CreateScope();
        var repository  = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        // ── Step 1: Confirm seeded data ────────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Clip 8 — Confirm Seeded Data"),
            ConsoleColor.DarkBlue);

        var specOrderSearchNoFilters = new OrderSearchSpecification(null, null);
        var allOrders                = await repository.ListAsync(specOrderSearchNoFilters);

        //TODO: Module 6 Clip 8 — remove this block and uncomment the seeded-data block below
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This screen unlocks in Module 6 Clip 8 — remove this block and uncomment the block below.",
                "Prerequisite: having completed OrderSearchSpecification,",
                "and having completed OrderRepository.ListAsync (TODO: Module 4 Clip 4)."
            }
        ));

        //var totalCount     = allOrders.Count;
        //var placedCount    = allOrders.Count(o => o.Status == OrderStatus.Placed);
        //var shippedCount   = allOrders.Count(o => o.Status == OrderStatus.Shipped);
        //var cancelledCount = allOrders.Count(o => o.Status == OrderStatus.Cancelled);
        //var openCount      = allOrders.Count(o => o.Status != OrderStatus.Shipped && o.Status != OrderStatus.Cancelled);
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    $"Database State — {totalCount} Orders Seeded",
        //    new[]
        //    {
        //        $"Total orders:     {totalCount}",
        //        $"  Placed:         {placedCount}",
        //        $"  Shipped:        {shippedCount}",
        //        $"  Cancelled:      {cancelledCount}",
        //        $"  Open (not shipped/cancelled): {openCount}"
        //    }
        //));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Step 2: The named specification ────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 2 — Named Specification: OrdersByCustomerSpecification(customerId)"),
            ConsoleColor.DarkBlue);

        var specOrdersByCustomerA = new OrdersByCustomerSpecification(SeedDataHelper.CustomerAId);
        var customerAOrders       = await repository.ListAsync(specOrdersByCustomerA);

        //TODO: Module 6 Clip 8 — remove this block and uncomment the named-specification block below
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This screen unlocks in Module 6 Clip 8 — remove this block and uncomment the block below.",
                "Prerequisite: having completed Include/OrderBy on OrdersByCustomerSpecification,",
                "and having completed OrderRepository.ListAsync (TODO: Module 4 Clip 4)."
            }
        ));

        //var customerAOrderRows = customerAOrders.Select(o =>
        //    $"    Id={o.Id}  " +
        //    $"Status={o.Status,-10}  " +
        //    $"Lines={o.Lines.Count}  " +
        //    $"PlacedAt={o.PlacedAt:yyyy-MM-dd HH:mm}")
        //    .ToArray();
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    $"OrdersByCustomerSpecification — {customerAOrders.Count} result(s) for Customer {SeedDataHelper.CustomerAId}",
        //    new[]
        //    {
        //        $"Predicate: EF.Property<int>(o, \"CustomerId\") == {SeedDataHelper.CustomerAId}",
        //        "Include:   Lines (eager-loaded — Lines.Count reflects real data)",
        //        "Order:     Descending by PlacedAt",
        //        "---"
        //    }
        //    .Concat(customerAOrderRows)
        //    .Concat(new[]
        //    {
        //        "---",
        //        $"All returned orders have Lines.Count > 0: {customerAOrders.All(o => o.Lines.Count > 0)}"
        //    })
        //    .ToArray()
        //));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Step 3: Dynamic search, one filter ────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 3 — Dynamic Search, One Filter: OrderSearchSpecification(Placed, null)"),
            ConsoleColor.DarkBlue);

        var specOrderSearchPlacedStatus = new OrderSearchSpecification(OrderStatus.Placed, null);
        var placedOrders                = await repository.ListAsync(specOrderSearchPlacedStatus);

        //TODO: Module 6 Clip 8 — remove this block and uncomment the one-filter search block below
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This screen unlocks in Module 6 Clip 8 — remove this block and uncomment the block below.",
                "Prerequisite: having completed OrderSearchSpecification,",
                "and having completed OrderRepository.ListAsync (TODO: Module 4 Clip 4)."
            }
        ));

        //var placedOrderRows = placedOrders.Select(o =>
        //    $"    Id={o.Id}  " +
        //    $"Status={o.Status,-10}  " +
        //    $"Lines={o.Lines.Count}  " +
        //    $"PlacedAt={o.PlacedAt:yyyy-MM-dd}")
        //    .ToArray();
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    $"OrderSearchSpecification(Placed, null) — {placedOrders.Count} result(s)",
        //    new[]
        //    {
        //        "status:   Placed  — WHERE o.Status == Placed",
        //        "minLines: null    — no line count filter",
        //        "---"
        //    }
        //    .Concat(placedOrderRows)
        //    .ToArray()
        //));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Step 4: Dynamic search, both filters ──────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 4 — Dynamic Search, Both Filters: OrderSearchSpecification(Shipped, 1)"),
            ConsoleColor.DarkBlue);

        var specOrderSearchShippedWithLines = new OrderSearchSpecification(OrderStatus.Shipped, 1);
        var shippedOrders                   = await repository.ListAsync(specOrderSearchShippedWithLines);

        //TODO: Module 6 Clip 8 — remove this block and uncomment the both-filters search block below
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This screen unlocks in Module 6 Clip 8 — remove this block and uncomment the block below.",
                "Prerequisite: having completed OrderSearchSpecification,",
                "and having completed OrderRepository.ListAsync (TODO: Module 4 Clip 4)."
            }
        ));

        //var shippedOrderRows = shippedOrders.Any()
        //    ? shippedOrders.Select(o =>
        //        $"    Id={o.Id}  " +
        //        $"Status={o.Status,-10}  " +
        //        $"Lines={o.Lines.Count}  " +
        //        $"PlacedAt={o.PlacedAt:yyyy-MM-dd}")
        //        .ToArray()
        //    : new[] { "    (no results)" };
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    $"OrderSearchSpecification(Shipped, 1) — {shippedOrders.Count} result(s)",
        //    new[]
        //    {
        //        "status:   Shipped  — WHERE o.Status == Shipped",
        //        "minLines: 1        — AND o.Lines.Count >= 1",
        //        "---"
        //    }
        //    .Concat(shippedOrderRows)
        //    .ToArray()
        //));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Step 5: End-to-end summary ─────────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 5 — End-to-End Complete: All Three Searches Verified"),
            ConsoleColor.DarkBlue);

        //TODO: Module 6 Clip 8 — remove this block and uncomment the summary block below
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This screen unlocks in Module 6 Clip 8 — remove this block and uncomment the block below.",
                "Prerequisite: having completed OrderSearchSpecification,",
                "and having completed OrderRepository.ListAsync (TODO: Module 4 Clip 4)."
            }
        ));

        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "Results Summary — One Repository Method, Three Specifications",
        //    new[]
        //    {
        //        $"Named spec           OrdersByCustomerSpecification({SeedDataHelper.CustomerAId}):  {customerAOrders.Count} order(s)  — Lines eager-loaded",
        //        $"Dynamic, one filter  OrderSearchSpec(Placed, null):           {placedOrders.Count} order(s)  — status filter only",
        //        $"Dynamic, two filters OrderSearchSpec(Shipped, 1):             {shippedOrders.Count} order(s)  — both filters",
        //        "",
        //        "repository.ListAsync(spec) was called identically in all three scenarios.",
        //        "The specification was the only variable — the repository never changed.",
        //        "",
        //        "Zero .Where() calls in the demo. Zero LINQ in the caller.",
        //        "",
        //        "Module 6 wrote five specification classes in total.",
        //        "Four answer a fixed question. One answers whatever the caller asks.",
        //        "OrderRepository still exposes the single ListAsync method it started with."
        //    }
        //));
    }
}
