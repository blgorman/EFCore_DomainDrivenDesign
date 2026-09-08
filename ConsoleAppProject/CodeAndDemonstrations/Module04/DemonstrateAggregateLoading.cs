using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateAggregateLoading
{
    public static async Task ShowIncludeStrategyAsync(IServiceProvider serviceProvider)
    {
        // ── SETUP ──────────────────────────────────────────────────────────────
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        await SeedDataHelper.SeedAsync(context);

        // Grab the first seeded order that has lines so the proof is meaningful
        var firstOrderId = await context.Orders
            .AsNoTracking()
            .Where(o => o.Lines.Any())
            .Select(o => o.Id)
            .FirstAsync();

        // ── SCENARIO A: WITH Include ───────────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Scenario A — Include(o => o.Lines): Loading the Full Aggregate"),
            ConsoleColor.DarkBlue);

        var orderWithLines = await context.Orders
            .Include(o => o.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == firstOrderId);

        var lineDetails = orderWithLines!.Lines
            .Select((l, i) =>
                $"  Line {i + 1}: ProductId={l.ProductId}  " +
                $"Qty={l.Quantity,2}   " +
                $"UnitPrice={l.UnitPrice.Amount,7:F2} {l.UnitPrice.Currency}")
            .ToArray();

        var scenarioAItems = new[]
            {
                $"Id:           {orderWithLines.Id}",
                $"Status:       {orderWithLines.Status}",
                $"Lines loaded: {orderWithLines.Lines.Count}  (all children present)",
                "---"
            }
            .Concat(lineDetails)
            .Concat(new[]
            {
                "---",
                "UnitPrice on each line is an owned entity — it is loaded from columns on",
                "the OrderLine row. The next screen shows that mapping."
            })
            .ToArray();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Order with Include(o => o.Lines) — Full Aggregate Graph",
            scenarioAItems));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── SCENARIO B: WITHOUT Include ────────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Scenario B — Omitting Include: Navigation Property Is Empty"),
            ConsoleColor.DarkBlue);

        // Fresh scope: no change-tracking left from Scope A to accidentally fill Lines
        using var scopeB = serviceProvider.CreateScope();
        var contextB = scopeB.ServiceProvider.GetRequiredService<OrderingContext>();

        var orderWithoutLines = await contextB.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == firstOrderId);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Order WITHOUT Include — Lines Collection Is Empty",
            new[]
            {
                $"Id:           {orderWithoutLines!.Id}",
                $"Status:       {orderWithoutLines.Status}",
                $"Lines.Count:  {orderWithoutLines.Lines.Count}" +
                    "   <-- 0, even though the rows exist in the database",
                "---",
                "EF Core did not issue a JOIN — no Include was requested.",
                "Lines.Count == 0 even though the seeded order has lines.",
                "Business rules that iterate Lines silently operate on an empty graph.",
                "This is why OrderRepository.GetByIdAsync always bakes in .Include(o => o.Lines)."
            }));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── SECTION C: OWNED ENTITIES ──────────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Owned Entities — Loaded From the Owner's Own Row"),
            ConsoleColor.DarkBlue);

        // Proof: UnitPrice is already populated from Scenario A, which only Included Lines
        var firstLine = orderWithLines.Lines.First();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "UnitPrice Is an Owned Entity, Not a Separate Table",
            new[]
            {
                "OrderLine.UnitPrice is a Money value object, mapped by OrderLineConfiguration",
                "with OwnsOne. EF maps an owned entity into columns on the owner's row:",
                "  OrderLine table, column UnitPrice_Amount     (decimal(18,2))",
                "  OrderLine table, column UnitPrice_Currency   (nvarchar(10))",
                "---",
                "Scenario A's query wrote one Include, for Lines, and nothing for UnitPrice:",
                "  context.Orders",
                "    .Include(o => o.Lines)",
                "    .FirstOrDefaultAsync(o => o.Id == id)",
                "---",
                "Proof — UnitPrice read off that same Scenario A object:",
                $"  Line 1 UnitPrice: {firstLine.UnitPrice.Amount:F2} {firstLine.UnitPrice.Currency}" +
                    "   <-- loaded with the OrderLine row"
            }));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── SECTION D: AsSplitQuery ────────────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("AsSplitQuery() — One JOIN or One SELECT per Collection"),
            ConsoleColor.DarkBlue);

        using var scopeD = serviceProvider.CreateScope();
        var contextD = scopeD.ServiceProvider.GetRequiredService<OrderingContext>();

        // Count the rows the Include JOIN actually returns for this one order
        var joinRowCount = await contextD.Database
            .SqlQuery<int>($@"
                SELECT COUNT(*) AS Value
                FROM Orders o
                LEFT JOIN OrderLine l ON l.OrderId = o.Id
                WHERE o.Id = {firstOrderId}")
            .SingleAsync();

        // Run the same load with AsSplitQuery() to prove it returns the same aggregate
        var orderViaSplitQuery = await contextD.Orders
            .Include(o => o.Lines)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == firstOrderId);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "AsSplitQuery() — Same Aggregate, Separate Round Trips",
            new[]
            {
                "Scenario A wrote one Include, so SQL issued one LEFT JOIN:",
                "  SELECT ... FROM Orders o LEFT JOIN OrderLine l ON l.OrderId = o.Id",
                "That returns one row per OrderLine, repeating the Order columns on each row.",
                $"  Rows returned by that JOIN for Order {firstOrderId}: {joinRowCount}" +
                    $"   <-- counted against the live database",
                $"  Lines on that order: {orderWithLines.Lines.Count}",
                "---",
                "AsSplitQuery() issues one SELECT per collection instead of one JOIN:",
                "  context.Orders",
                "    .Include(o => o.Lines)",
                "    .AsSplitQuery()            // two queries: SELECT Orders, then SELECT Lines",
                "    .FirstOrDefaultAsync(o => o.Id == id)",
                "---",
                "AsSplitQuery() result from the live database:",
                $"  Lines.Count: {orderViaSplitQuery!.Lines.Count}" +
                    "   <-- same aggregate as Scenario A",
                "---",
                "Trade-off: one extra round trip, and the Order columns are read once.",
                "Every additional collection navigation in one query multiplies the rows again.",
                "The next screen prints the SQL for both loads."
            }));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── SECTION E: THE GENERATED SQL ───────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Generated SQL — What EF Core Actually Sent"),
            ConsoleColor.DarkBlue);


        await using (var loggingContext = CreateLoggingContext(contextD))
        {
            OutputHelpers.WriteColored("  Load 1: .Include(o => o.Lines)", ConsoleColor.DarkGreen);
            Console.WriteLine();
            await loggingContext.Orders
                .Include(o => o.Lines)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == firstOrderId);

            Console.WriteLine();
            InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
            Console.WriteLine();

            OutputHelpers.WriteColored("  Load 2: .Include(o => o.Lines).AsSplitQuery()", ConsoleColor.DarkGreen);
            Console.WriteLine();
            await loggingContext.Orders
                .Include(o => o.Lines)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == firstOrderId);
        }

        Console.WriteLine();
    }

    private static OrderingContext CreateLoggingContext(OrderingContext source)
    {
        var options = new DbContextOptionsBuilder<OrderingContext>()
            .UseSqlServer(source.Database.GetConnectionString())
            .LogTo(
                Console.WriteLine,
                new[] { DbLoggerCategory.Database.Command.Name },
                LogLevel.Information)
            .Options;

        return new OrderingContext(options);
    }
}
