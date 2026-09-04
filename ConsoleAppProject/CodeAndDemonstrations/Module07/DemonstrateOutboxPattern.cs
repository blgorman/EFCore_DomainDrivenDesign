using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Outbox;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateOutboxPattern
{
    public static async Task ShowOutboxAsync(IServiceProvider serviceProvider)
    {
        // Seed reference data and capture the connection string for a local context
        string connectionString;
        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(seedCtx);
            connectionString = seedCtx.Database.GetConnectionString()!;
        }

        // ── Screen 1: The problem the outbox solves ───────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Module 7 — Transactional Outbox"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Post-save dispatch vs the outbox",
            new[]
            {
                "Module 5: DomainEventDispatchInterceptor dispatches in SavedChangesAsync,",
                "after the commit. A crash between the commit and the dispatch loses the",
                "event — the order exists, and no handler ever ran.",
                "",
                "The outbox: OutboxWriteInterceptor runs in SavingChangesAsync instead.",
                "Each domain event becomes an OutboxMessage row in the SAME save, so the",
                "event log commits atomically with the order. A relay publishes the rows",
                "afterward, and unpublished rows survive any crash."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // Local context: same database, OutboxWriteInterceptor instead of the
        // dispatch interceptor. The OutboxMessages table is created here at runtime —
        // it is not part of the Ordering migrations.
        var outboxContextOptions = new DbContextOptionsBuilder<OrderingContext>()
            .UseSqlServer(connectionString)
            .AddInterceptors(new OutboxWriteInterceptor())
            .Options;

        // Course demo only, not a production-ready outbox. A production outbox creates
        // the OutboxMessages table in a migration. The raw CREATE TABLE below keeps the
        // schema for Modules 1 through 6 exactly as those recordings show, and the
        // database rebuild on the next application start removes the table.
        await using var outboxContext = new OutboxOrderingContext(outboxContextOptions);
        await outboxContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
            CREATE TABLE dbo.OutboxMessages (
                Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OutboxMessages PRIMARY KEY,
                Type NVARCHAR(200) NOT NULL,
                Payload NVARCHAR(MAX) NOT NULL,
                OccurredAtUtc DATETIME2 NOT NULL,
                ProcessedAtUtc DATETIME2 NULL
            );
            """);

        // ── Screen 2: One save, two kinds of rows ─────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("One SaveChangesAsync, the Order Row and the Event Row"),
            ConsoleColor.DarkBlue);

        var order = Order.Place(SeedDataHelper.CustomerAId, new[]
        {
            (SeedDataHelper.Product1Id, 2, Money.Create(29.99m, "USD"))
        });
        outboxContext.Orders.Add(order);
        outboxContext.Entry(order).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
        await outboxContext.SaveChangesAsync();

        var unpublishedMessages = await outboxContext.OutboxMessages
            .Where(m => m.ProcessedAtUtc == null)
            .ToListAsync();

        var committedRows = unpublishedMessages
            .Select(m => $"Row {m.Id}: {m.Type} at {m.OccurredAtUtc:HH:mm:ss.fff} — {m.Payload}")
            .Concat(new[]
            {
                "",
                $"Order {order.Id} and {unpublishedMessages.Count} OutboxMessage row(s) committed together.",
                "No handler ran. The event is durable data now, not an in-memory callback.",
                "If the process dies here, the row is still in the table after restart."
            })
            .ToArray();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OutboxMessages rows committed with the order",
            committedRows
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // ── Screen 3: The relay pass ──────────────────────────────────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("The Relay Pass — Publish, Then Mark Processed"),
            ConsoleColor.DarkBlue);

        var relayLog = new List<string>();
        foreach (var message in unpublishedMessages)
        {
            // A production relay is a hosted service on a timer publishing to a broker
            // such as MassTransit or NServiceBus. This pass simulates one cycle.
            relayLog.Add($"Published {message.Type} (row {message.Id}) to the broker (simulated).");
            message.ProcessedAtUtc = DateTime.UtcNow;
        }
        await outboxContext.SaveChangesAsync();

        var remainingUnpublishedCount = await outboxContext.OutboxMessages.CountAsync(m => m.ProcessedAtUtc == null);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "One relay cycle",
            relayLog
                .Concat(new[]
                {
                    "",
                    $"Unpublished rows remaining: {remainingUnpublishedCount}.",
                    "ProcessedAtUtc marks delivery — a crashed relay re-reads unmarked rows",
                    "on its next pass, which is what makes delivery at-least-once.",
                    "The interceptor from Module 5 did not move. Only its body changed.",
                    "",
                    "MassTransit and NServiceBus both ship a transactional outbox.",
                    "This one is hand-rolled to show the mechanism; production adopts theirs."
                })
                .ToArray()
        ));

        await Task.CompletedTask;
    }
}
