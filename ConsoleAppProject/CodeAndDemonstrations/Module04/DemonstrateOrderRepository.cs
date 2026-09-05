using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateOrderRepository
{
    public static async Task WalkImplementationAsync(IServiceProvider serviceProvider)
    {
        // Seed in its own scope so demonstration scopes start with a clean ChangeTracker.
        //SeedDataHelper.CustomerAId stays 0 until this runs, and Order.Place rejects 0.
        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(seedCtx);
        }

        // --- PART 1: Constructor injection and private context field ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Implementing the Repository — Constructor and Private Context Field"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderRepository — class declaration and constructor",
            new[]
            {
                "public class OrderRepository : IOrderRepository",
                "{",
                "    private readonly OrderingContext _ctx;",
                "",
                "    public OrderRepository(OrderingContext ctx) => _ctx = ctx;",
                "",
                "    // Five IOrderRepository methods implemented below...",
                "}",
                "",
                "Key points:",
                "  _ctx is private readonly — nothing outside this class can reach OrderingContext",
                "  Constructor injection: the DI container supplies OrderingContext automatically",
                "  OrderingContext is Scoped in DI, so one instance per request/scope"
            }
        ));

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // --- PART 1 live proof: verifying all three key points ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 1 Continued — Proving All Three Key Points at Runtime"), ConsoleColor.DarkBlue);

        // Proof 1 — private and readonly
        using (var proof1Scope = serviceProvider.CreateScope())
        {
            var repo           = proof1Scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var publicLookup   = repo.GetType().GetField("_ctx", BindingFlags.Public | BindingFlags.Instance);
            var privateLookup  = repo.GetType().GetField("_ctx", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Readonly: scan every public instance method for any parameter that accepts OrderingContext
            var swapMethods = repo.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetParameters().Any(p => p.ParameterType == typeof(OrderingContext)))
                .Select(m => m.Name)
                .ToArray();

            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "Proof 1 — _ctx is private: public lookup returns null",
                new[]
                {
                    "GetField(\"_ctx\", BindingFlags.Public):",
                    $"  => {(publicLookup == null ? "null — no such field exists on the public surface" : publicLookup.Name)}",
                    "",
                    "GetField(\"_ctx\", BindingFlags.NonPublic):",
                    $"  => {privateLookup.Name} — only reachable when explicitly requesting non-public members",
                    "",
                    "Scan every public instance method for an OrderingContext parameter:",
                    $"  methods found: {swapMethods.Length}",
                    $"  {(swapMethods.Length == 0 ? "=> Zero. There is no public method that accepts a new OrderingContext." : string.Join(", ", swapMethods))}",
                    "",
                    "private: callers cannot name the field.",
                    "readonly: the compiler rejects any assignment to _ctx outside of the",
                    "          constructor body — and no public method exists to do it indirectly."
                }
            ));
        }

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // Proof 2 — DI injection: ChangeTracker contamination test
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Proof 2 — DI Injected the Exact Same Instance: ChangeTracker Contamination Test"), ConsoleColor.DarkBlue);
        using (var proof2Scope = serviceProvider.CreateScope())
        {
            var ctxDirect = proof2Scope.ServiceProvider.GetRequiredService<OrderingContext>();
            var repo      = proof2Scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var ctxField  = repo.GetType().GetField("_ctx", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Add an Order directly to ctxDirect — completely bypassing the repository's Add() method
            var order = Order.Place(SeedDataHelper.CustomerAId, new[]
            {
                (SeedDataHelper.Product1Id, 1, Money.Create(9.99m, "USD"))
            });
            ctxDirect.Orders.Add(order);

            var repoCtx           = ctxField.GetValue(repo) as OrderingContext;
            var entriesViaDirect  = ctxDirect.ChangeTracker.Entries<Order>().Count();
            var entriesViaRepo    = repoCtx!.ChangeTracker.Entries<Order>().Count();

            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "Proof 2 — add directly to ctxDirect; read ChangeTracker through repo._ctx",
                new[]
                {
                    "ctxDirect.Orders.Add(newOrder) — added via the raw DbContext, NOT through repo.Add().",
                    "repo.Add() was never called. repo.SaveAsync() was never called.",
                    "",
                    $"ctxDirect.ChangeTracker.Entries<Order>().Count():  {entriesViaDirect}",
                    $"repo._ctx.ChangeTracker.Entries<Order>().Count():  {entriesViaRepo}",
                    "",
                    "Both show 1 pending entry.",
                    "The repo's internal DbContext already sees the Order even though",
                    "nothing was ever passed through the repository.",
                    "The only explanation: ctxDirect and repo._ctx are the same object.",
                    "DI handed the repository the instance it already owned."
                }
            ));
        }

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        // Proof 3a — same scope, same DbContext
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Proof 3a — Scoped Lifetime: Same Scope, Same DbContext Instance"), ConsoleColor.DarkBlue);
        using (var proof3aScope = serviceProvider.CreateScope())
        {
            var repoX    = proof3aScope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var repoY    = proof3aScope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var ctxField = repoX.GetType().GetField("_ctx", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var ctxX     = ctxField.GetValue(repoX);
            var ctxY     = ctxField.GetValue(repoY);

            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "Proof 3a — Scoped: same scope supplies the same OrderingContext instance",
                new[]
                {
                    "Resolve IOrderRepository twice inside one scope:",
                    $"  ReferenceEquals(repoX._ctx, repoY._ctx):  {ReferenceEquals(ctxX, ctxY)}",
                    "",
                    "True — both repositories share the exact same DbContext instance.",
                    "One DbContext per scope is the Scoped lifetime guarantee."
                }
            ));
        }

        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Proof 3b — Scoped Isolation: Changes in Scope A Are Invisible to Scope B"), ConsoleColor.DarkBlue);

        using (var scopeA = serviceProvider.CreateScope())
        using (var scopeB = serviceProvider.CreateScope())
        {
            var ctxA  = scopeA.ServiceProvider.GetRequiredService<OrderingContext>();
            var repoA = scopeA.ServiceProvider.GetRequiredService<IOrderRepository>();
            var ctxB  = scopeB.ServiceProvider.GetRequiredService<OrderingContext>();

            var pendingOrder = Order.Place(SeedDataHelper.CustomerAId, new[]
            {
                (SeedDataHelper.Product1Id, 1, Money.Create(9.99m, "USD"))
            });
            repoA.Add(pendingOrder);

            var scopeAPending = ctxA.ChangeTracker.Entries<Order>().Count();
            var scopeBPending = ctxB.ChangeTracker.Entries<Order>().Count();

            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "Proof 3b — ChangeTracker isolation: scope A's unsaved work is invisible to scope B",
                new[]
                {
                    "Action: call repoA.Add(newOrder) — no SaveAsync, no INSERT yet.",
                    "",
                    $"Scope A ChangeTracker.Entries<Order>().Count():  {scopeAPending}",
                    $"  => {scopeAPending}: the new Order is staged as EntityState.Added in scope A's DbContext.",
                    "",
                    $"Scope B ChangeTracker.Entries<Order>().Count():  {scopeBPending}",
                    $"  => {scopeBPending}: scope B's DbContext has no knowledge of scope A's pending work.",
                    "",
                    "Scope B cannot see, undo, or accidentally save what scope A is doing.",
                    "This is not a hash comparison — it is a live ChangeTracker state read."
                }
            ));
        }

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 2: The five implementation methods ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("The Five Methods — All Delegating to the Private _ctx"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "GetByIdAsync — always loads the full aggregate graph",
            new[]
            {
                "public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)",
                "{",
                "    return await _ctx.Orders",
                "        .Include(o => o.Lines)",
                "        .FirstOrDefaultAsync(o => o.Id == id, ct);",
                "}"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Add and Remove — change tracking only, no SQL yet",
            new[]
            {
                "public void Add(Order order)    =>  _ctx.Orders.Add(order);",
                "public void Remove(Order order) =>  _ctx.Orders.Remove(order);",
                "",
                "public void Add(Order order, int customerId)",
                "{",
                "    _ctx.Orders.Add(order);",
                "    _ctx.Entry(order).Property<int>(\"CustomerId\").CurrentValue = customerId;",
                "}",
                "",
                "Order stopped storing CustomerId as a mapped property in Module 2 Clip 5.",
                "OrderRepository holds the OrderingContext, so the overload sets the shadow FK.",
                "",
                "No SQL is issued. SQL only reaches the database when SaveAsync is called."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "SaveAsync — flushes all pending ChangeTracker entries",
            new[]
            {
                "public async Task SaveAsync(CancellationToken ct = default)",
                "{",
                "    await _ctx.SaveChangesAsync(ct);",
                "}"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "ListAsync — Ardalis.Specification builds the IQueryable",
            new[]
            {
                "public async Task<IReadOnlyList<Order>> ListAsync(",
                "    ISpecification<Order> spec, CancellationToken ct = default)",
                "{",
                "    var query = SpecificationEvaluator.Default",
                "        .GetQuery(_ctx.Orders.AsQueryable(), spec);",
                "    return await query.ToListAsync(ct);",
                "}",
                "",
                "Callers pass a typed Specification object — no raw IQueryable escapes.",
                "Module 6 covers the Specification pattern in full detail."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 3: Live call — GetByIdAsync against seeded data ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Live Demo — GetByIdAsync Against Seeded Data"), ConsoleColor.DarkBlue);

        using (var liveScope = serviceProvider.CreateScope())
        {
            var ctx  = liveScope.ServiceProvider.GetRequiredService<OrderingContext>();
            var repo = liveScope.ServiceProvider.GetRequiredService<IOrderRepository>();

            await SeedDataHelper.SeedAsync(ctx);

            var firstId = await ctx.Orders.AsNoTracking().Select(o => o.Id).FirstAsync();
            var loaded  = await repo.GetByIdAsync(firstId);

            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "GetByIdAsync — real result from the database",
                new[]
                {
                    $"Id:         {loaded!.Id}",
                    $"CustomerId: {ctx.Entry(loaded).Property<int>("CustomerId").CurrentValue}",
                    $"Status:     {loaded.Status}",
                    $"PlacedAt:   {loaded.PlacedAt:u}",
                    $"Lines:      {loaded.Lines.Count}  (fully loaded — Include is baked in)"
                }
            ));
        }
    }
}
