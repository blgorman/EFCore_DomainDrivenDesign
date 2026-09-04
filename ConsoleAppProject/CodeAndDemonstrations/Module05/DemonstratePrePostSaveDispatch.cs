using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Events;
using OrderManagement.Infrastructure.Repositories;
using System.Reflection;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstratePrePostSaveDispatch
{
    public static async Task CompareDispatchTimingAsync(IServiceProvider serviceProvider)
    {
        string connectionString;
        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(seedCtx);
            connectionString = seedCtx.Database.GetConnectionString()!;
        }

        // ── Step 1: Pre-save vs post-save — the two override points ───────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Clip 5 — Pre-Save vs Post-Save Domain Event Dispatch"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Two Intercept Points in SaveChangesInterceptor",
            new[]
            {
                "Pre-Save  — override SavingChangesAsync:",
                "  Runs BEFORE EF Core issues any SQL to the database.",
                "  Handler runs inside the transaction window.",
                "  If the handler throws an exception, SaveChanges throws and the write rolls back.",
                "  Use when: handler failure MUST prevent the DB write (strong consistency).",
                "",
                "Post-Save — override SavedChangesAsync:",
                "  Runs AFTER EF Core commits the transaction.",
                "  Handler runs outside the transaction window.",
                "  If the handler throws an exception, the DB row already exists — no rollback.",
                "  Use when: DB write must always succeed; eventual consistency is acceptable."
            }
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 2: Reflection proof — which override is active ───────────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 2 — Reflection Proof: Which Override Is Active?"),
            ConsoleColor.DarkBlue);

        var interceptorType = typeof(DomainEventDispatchInterceptor);
        var declaredMethods = interceptorType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .ToHashSet();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "DomainEventDispatchInterceptor — Declared Overrides",
            new[]
            {
                $"SavingChangesAsync declared (pre-save):  {declaredMethods.Contains("SavingChangesAsync")}",
                $"SavedChangesAsync  declared (post-save): {declaredMethods.Contains("SavedChangesAsync")}"
            }
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // ── Step 3: One save — dispatch, clear, and commit measured ───────────
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Step 3 — One Save: Dispatch, Clear, and Commit"),
            ConsoleColor.DarkBlue);

        //TODO: Module 5 Clip 5 — Delete this Not Yet Implemented box:
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This demo will work after completing Module 5 Clip 5.",
                "Open DemonstratePrePostSaveDispatch.cs and uncomment the //TODO: Module 5 Clip 5 block.",
                "Prerequisite: requires having completed Module 5 Clip 3 and Clip 4 — interceptor, dispatcher, and handlers."
            }
        ));

        //TODO: Module 5 Clip 5 — Uncomment the Step 3 measurement block below:
        //var handlerOutput = new List<string>();
        //using var dispatchProvider = CreateCapturingProvider(connectionString, handlerOutput);
        //using var dispatchScope = dispatchProvider.CreateScope();
        //
        //var context = dispatchScope.ServiceProvider.GetRequiredService<OrderingContext>();
        //var repo = dispatchScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //
        //var order = Order.Place(
        //    SeedDataHelper.CustomerAId,
        //new[] { (SeedDataHelper.Product1Id, 1, Money.Create(29.99m, "USD")) });
        //
        //var eventsBefore = order.DomainEvents.Count;
        //
        //repo.Add(order, SeedDataHelper.CustomerAId);
        //await repo.SaveAsync();
        //
        //var eventsAfter = order.DomainEvents.Count;
        //var savedRow = await context.Orders.FindAsync(order.Id);
        //var handlerLine = handlerOutput.Count > 0 ? handlerOutput[^1] : NoHandlerOutput;
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //"One Save — Measured",
        //new[]
        //{
        //    $"Events on aggregate before save: {eventsBefore}",
        //    $"Events on aggregate after save:  {eventsAfter}",
        //    $"DB row exists after save:        {savedRow is not null}",
        //    $"Saved row Id:                    {savedRow?.Id}",
        //    "",
        //    "Handler output:",
        //    $"  {handlerLine}"
        //}
        //));
        //
        //if (savedRow is not null)
        //{
        //    repo.Remove(savedRow);
        //await repo.SaveAsync();
        //}
    }

    private const string NoHandlerOutput = "(handler produced no output)";

    // Mirrors the domain event registrations in Program.cs, with the handler's logger
    // routed into a list so the demo can print what the handler wrote inside a box.
    private static ServiceProvider CreateCapturingProvider(string connectionString, List<string> handlerOutput)
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(LogCaptureHelpers.CaptureFor(nameof(OrderPlacedEventHandler), handlerOutput));
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddScoped<DomainEventDispatcher>();
        services.AddScoped<DomainEventDispatchInterceptor>();
        services.AddScoped<IDomainEventHandler<OrderPlacedEvent>, OrderPlacedEventHandler>();

        services.AddDbContext<OrderingContext>((sp, options) =>
            options.UseSqlServer(connectionString)
                   .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IOrderRepository, OrderRepository>();

        return services.BuildServiceProvider();
    }
}
