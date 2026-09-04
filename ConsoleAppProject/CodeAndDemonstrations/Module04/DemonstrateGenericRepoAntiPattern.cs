using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateGenericRepoAntiPattern
{
    // ── The anti-pattern being demonstrated ─────────────────────────────────
    // GenericRepository<T> is the class many teams write to "reduce boilerplate".
    // It lives here in the demo so we can run it and watch the problems happen.
    private sealed class GenericRepository<T> where T : class
    {
        private readonly OrderingContext _ctx;
        public GenericRepository(OrderingContext ctx) => _ctx = ctx;

        public async Task<T?> GetByIdAsync(int id)
            => await _ctx.Set<T>().FindAsync(id);

        public async Task<IReadOnlyList<T>> GetAllAsync()
            => await _ctx.Set<T>().ToListAsync();   // SELECT * — no filter, no limit

        public void Add(T entity) => _ctx.Set<T>().Add(entity);

        public void Remove(T entity) => _ctx.Set<T>().Remove(entity);

        public Task UpdateAsync(T entity)
        {
            _ctx.Update(entity);      // marks EVERY property Modified
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync() => await _ctx.SaveChangesAsync();
    }
    // ────────────────────────────────────────────────────────────────────────

    public static async Task ContrastApproachesAsync(IServiceProvider serviceProvider)
    {
        // ── Seed data once ───────────────────────────────────────────────────
        int orderId;
        using (var seedScope = serviceProvider.CreateScope())
        {
            var ctx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(ctx);

            // Save one extra order so we have a known ID to work with
            var repo = new GenericRepository<Order>(ctx);
            var order = Order.Place(
                SeedDataHelper.CustomerAId,
                new[] { (SeedDataHelper.Product1Id, 1, Money.Create(25m, "USD")) }
            );
            repo.Add(order);
            //Order.Place() does not set the CustomerId shadow FK; set it on the tracked entity.
            ctx.Entry(order).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
            await repo.SaveChangesAsync();
            orderId = order.Id;
        }
        // ────────────────────────────────────────────────────────────────────

        // --- PART 3: Problem 1 — UpdateAsync marks ALL columns dirty ---
        using (var scope = serviceProvider.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();
            var repo = new GenericRepository<Order>(ctx);

            var order = await repo.GetByIdAsync(orderId);

            // ── Before UpdateAsync ────────────────────────────────────────
            var entry = ctx.Entry(order!);
            var modifiedBefore = entry.Properties
                .Where(p => p.IsModified)
                .Select(p => p.Metadata.Name)
                .DefaultIfEmpty("(none — EF only tracks actual changes)")
                .ToArray();

            OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 3 — ChangeTracker BEFORE UpdateAsync"), ConsoleColor.DarkBlue);
            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                $"Order loaded. Entity state: {entry.State}. Modified properties:",
                modifiedBefore
            ));

            //----------------------------------------------------------------//
            Console.WriteLine();
            InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
            Console.WriteLine();
            //----------------------------------------------------------------//

            // ── Call UpdateAsync — NO mutations made ──────────────────────
            await repo.UpdateAsync(order!);   // <-- the anti-pattern in action

            var modifiedAfter = entry.Properties
                .Where(p => p.IsModified)
                .Select(p => p.Metadata.Name)
                .ToArray();

            OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 3 — ChangeTracker AFTER UpdateAsync (nothing changed)"), ConsoleColor.DarkBlue);
            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                $"Same order, zero mutations. Entity state: {entry.State}. Modified properties:",
                modifiedAfter
            ));

            //----------------------------------------------------------------//
            Console.WriteLine();
            InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
            Console.WriteLine();
            //----------------------------------------------------------------//

            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "What SaveChanges would emit now",
                new[]
                {
                    "UPDATE Orders SET",
                    "    CustomerId    = @p0,   <-- unchanged",
                    "    Status        = @p1,   <-- unchanged",
                    "    PlacedAt      = @p2    <-- unchanged",
                    "WHERE Id = @p3",
                    "",
                    "Every column flagged. Full-row UPDATE on an unmodified entity.",
                    "This is unnecessary I/O generated by the 'UpdateAsync' method.",
                    "A caller using the generic repo is FORCED to understand that",
                    "calling UpdateAsync when not needed is a bug."
                }
            ));
        }

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 3b: Problem 1 in production — the lost update ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 3b: The Lost Update — UpdateAsync Overwrites Another User's Change"), ConsoleColor.DarkBlue);
        //TODO: Module 4 Clip 1 — Delete the "Not Yet Implemented" box below.
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This screen will work after completing Module 4 Clip 1.",
                "Open DemonstrateGenericRepoAntiPattern.cs and uncomment the two",
                "//TODO: Module 4 Clip 1 blocks: the lost-update walkthrough and ReadStatusAsync."
            }
        ));

        //TODO: Module 4 Clip 1 — Uncomment the lost-update walkthrough below.
        //using (var scopeA = serviceProvider.CreateScope())
        //{
        //    var ctxA  = scopeA.ServiceProvider.GetRequiredService<OrderingContext>();
        //    var repoA = new GenericRepository<Order>(ctxA);
        //
        //    // Our code loads the order and holds it.
        //    var staleOrder       = await repoA.GetByIdAsync(orderId);
        //    var statusWhenLoaded = staleOrder!.Status;
        //
        //    // A shipping clerk ships the same order from a completely separate scope.
        //    using (var clerkScope = serviceProvider.CreateScope())
        //    {
        //        var clerkCtx   = clerkScope.ServiceProvider.GetRequiredService<OrderingContext>();
        //        var clerkOrder = await clerkCtx.Orders.FirstAsync(o => o.Id == orderId);
        //        clerkOrder.Process();
        //        clerkOrder.Confirm();
        //        clerkOrder.Ship();
        //        await clerkCtx.SaveChangesAsync();
        //    }
        //
        //    var statusAfterClerk = await ReadStatusAsync(serviceProvider, orderId);
        //
        //    Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //        "Two users, one order",
        //        new[]
        //        {
        //            $"Our code loaded order {orderId} and still holds Status = {statusWhenLoaded}.",
        //            $"The shipping clerk saved order {orderId}, so the database now holds Status = {statusAfterClerk}.",
        //            "",
        //            "Our copy is stale, and our code never touched Status."
        //        }
        //    ));
        //
        //    //----------------------------------------------------------------//
        //    Console.WriteLine();
        //    InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //    Console.WriteLine();
        //    //----------------------------------------------------------------//
        //
        //    await repoA.UpdateAsync(staleOrder);
        //    await repoA.SaveChangesAsync();
        //
        //    var statusAfterOurSave = await ReadStatusAsync(serviceProvider, orderId);
        //
        //    Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //        "After our UpdateAsync and SaveChangesAsync",
        //        new[]
        //        {
        //            $"The database now holds Status = {statusAfterOurSave}.",
        //            "",
        //            "UpdateAsync flagged every column, and Status was one of them.",
        //            $"SaveChanges wrote our stale {statusWhenLoaded} over the clerk's {statusAfterClerk}.",
        //            "The shipment is gone from the database, and no exception was raised."
        //        }
        //    ));
        //}

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 4: Problem 2 — GetAllAsync returns everything ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 4: Problem 2 — GetAllAsync Has No Bounds"), ConsoleColor.DarkBlue);

        using (var scope = serviceProvider.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();
            var repo = new GenericRepository<Order>(ctx);

            // Call with zero filtering — SELECT * FROM Orders
            var allOrders = await repo.GetAllAsync();

            // The "workaround" pattern — filter in memory after loading everything
            var placed = allOrders.Where(o => o.Status == OrderManagement.Domain.Enums.OrderStatus.Placed).ToList();

            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                $"repo.GetAllAsync() returned {allOrders.Count} order(s) — no WHERE, no limit",
                allOrders.Select((o, i) =>
                    $"  [{i + 1}]  Id: {o.Id}  Status: {o.Status}  PlacedAt: {o.PlacedAt:u}"
                )
                .Concat(new[]
                {
                    "",
                    "The caller filters in memory after loading everything:",
                    "    var allOrders = await repo.GetAllAsync();",
                    "    var placed    = allOrders.Where(o => o.Status == Placed).ToList();",
                    "",
                    $"Rows fetched from the database:  {allOrders.Count}",
                    $"Rows actually needed:            {placed.Count}",
                    $"Rows fetched and discarded:      {allOrders.Count - placed.Count}",
                    "",
                    "Adding a filtered overload to the generic interface moves the query",
                    "logic to the caller, which is the work a Repository is meant to hold."
                })
                .ToArray()
            ));
        }

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 5: Problem 3 — T breaks aggregate boundaries ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 5: Problem 3 — GenericRepository<OrderLine> Compiles Fine"), ConsoleColor.DarkBlue);

        using (var scope = serviceProvider.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();

            // This is the problem: nothing stops anyone instantiating a repo for OrderLine.
            // OrderLine is NOT an Aggregate Root — it has no independent lifecycle.
            // It must only be created, modified, or removed through Order.
            var orderLineRepo = new GenericRepository<OrderLine>(ctx);

            // Load all lines directly — no Order involved at all
            var allLines = await orderLineRepo.GetAllAsync();

            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                $"new GenericRepository<OrderLine>(ctx).GetAllAsync() — {allLines.Count} line(s) returned",
                allLines.Select((l, i) =>
                    $"  [{i + 1}]  ProductId: {l.ProductId}  Qty: {l.Quantity}  Price: {l.UnitPrice.Amount:C}"
                )
                .Prepend("GenericRepository<OrderLine> compiled. It ran. No Order was involved.")
                .Concat(new[]
                {
                    "",
                    "With this repository a caller can also:",
                    "  - Delete a line with repo.Remove(line) and SaveChangesAsync(),",
                    "    dropping it from the order with no method on Order ever running",
                    "  - Load and hold OrderLine rows with no Order in scope at all",
                    "",
                    "Order is where the rules for its lines live, and this repository reaches",
                    "OrderLine without going through Order. Nothing here fails to compile."
                })
                .ToArray()
            ));
        }

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 6: What IOrderRepository removes ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 6: The Contrast — What IOrderRepository Removes"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "IOrderRepository — what changed and why each change matters",
            new[]
            {
                "GenericRepository<T>           IOrderRepository",
                "──────────────────────────────  ───────────────────────────────",
                "UpdateAsync(entity)         ->  removed — EF change tracking only",
                "GetAllAsync()               ->  removed — ListAsync(spec) replaces it",
                "Works for any type T        ->  only Order — no IOrderRepository<T>",
                "GenericRepo<OrderLine> OK   ->  no IOrderLineRepository exists",
                "SaveChangesAsync()          ->  SaveAsync() — same intent, scoped name",
                "",
                "The caller now cannot call UpdateAsync — because there is nothing to call.",
                "The caller cannot load without a filter — because GetAllAsync is gone.",
                "The caller cannot bypass Order — because only Order has a repository."
            }
        ));
    }

    //TODO: Module 4 Clip 1 — Uncomment the ReadStatusAsync helper below.
    //private static async Task<OrderManagement.Domain.Enums.OrderStatus> ReadStatusAsync(
    //    IServiceProvider serviceProvider, int orderId)
    //{
    //    using var scope = serviceProvider.CreateScope();
    //    var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();
    //    var order = await ctx.Orders.AsNoTracking().FirstAsync(o => o.Id == orderId);
    //    return order.Status;
    //}
}
