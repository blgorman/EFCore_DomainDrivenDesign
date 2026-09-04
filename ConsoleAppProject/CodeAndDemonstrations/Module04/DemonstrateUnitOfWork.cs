using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateUnitOfWork
{
    public static async Task ShowUoWBehaviorAsync(IServiceProvider serviceProvider)
    {
        //TODO: Module 4 Clip 7 — Delete the "Not Yet Implemented" box below.
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This demo will work after completing Module 4 Clip 7.",
                "Open DemonstrateUnitOfWork.cs and uncomment the //TODO: Module 4 Clip 7 block.",
                "Prerequisite: OrderRepository must already implement Add, SaveAsync, GetByIdAsync, and Remove (Module 4 Clip 4)."
            }
        ));

        //TODO: Module 4 Clip 7 — Uncomment the Unit of Work walkthrough below.
        //// Seed in its own scope so demonstration scopes start with a clean ChangeTracker
        //using (var seedScope = serviceProvider.CreateScope())
        //{
        //    var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
        //    await SeedDataHelper.SeedAsync(seedCtx);
        //}
        //
        //// ── PART 1: Two Add() calls — same DbContext, zero SQL ────────────────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Part 1 — Two Add() Calls, One DbContext, Zero SQL"),
        //    ConsoleColor.DarkBlue);
        //
        //using var stagingScope = serviceProvider.CreateScope();
        //var stagingCtx  = stagingScope.ServiceProvider.GetRequiredService<OrderingContext>();
        //var stagingRepo = stagingScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //
        //var order1 = Order.Place(SeedDataHelper.CustomerAId, new[]
        //{
        //    (SeedDataHelper.Product1Id, 2, Money.Create(29.99m, "USD"))
        //});
        //var order2 = Order.Place(SeedDataHelper.CustomerBId, new[]
        //{
        //    (SeedDataHelper.Product2Id, 1, Money.Create(49.99m, "USD"))
        //});
        //
        //// The overload sets the CustomerId shadow FK, which Order.Place() never sets.
        //stagingRepo.Add(order1, SeedDataHelper.CustomerAId);
        //stagingRepo.Add(order2, SeedDataHelper.CustomerBId);
        //
        //var pendingEntries = stagingCtx.ChangeTracker.Entries<Order>()
        //    .Where(e => e.Entity.Id == order1.Id || e.Entity.Id == order2.Id)
        //    .ToList();
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "stagingRepo.Add(order1, custA) + stagingRepo.Add(order2, custB) — ChangeTracker before SaveAsync",
        //    new[]
        //    {
        //        "Two Add() calls. No SaveAsync() yet.",
        //        "",
        //        $"  ChangeTracker.Entries<Order>() for these two:  {pendingEntries.Count}",
        //        $"  Order 1 EntityState:  {pendingEntries.First(e => e.Entity.Id == order1.Id).State}",
        //        $"  Order 2 EntityState:  {pendingEntries.First(e => e.Entity.Id == order2.Id).State}",
        //        "",
        //        "Both orders are staged in the same DbContext instance — no INSERT has been issued.",
        //        "services.AddScoped<IOrderRepository, OrderRepository>() and",
        //        "services.AddDbContext<OrderingContext>(...) are both Scoped.",
        //        "Within one scope, the repository and context are the same instance.",
        //        "The DbContext IS the Unit of Work — it accumulates all pending changes.",
        //        "One SaveAsync() will flush both as a single atomic database transaction."
        //    }));
        //
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        //
        //// ── PART 2: SaveAsync() — single atomic commit ────────────────────────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Part 2 — SaveAsync(): One Call, One Transaction, Both Committed"),
        //    ConsoleColor.DarkBlue);
        //
        //await stagingRepo.SaveAsync();
        //
        //// Fresh scope — proves both truly reached the database
        //using var readBackScope = serviceProvider.CreateScope();
        //var readBackRepo = readBackScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //var loaded1      = await readBackRepo.GetByIdAsync(order1.Id);
        //var loaded2      = await readBackRepo.GetByIdAsync(order2.Id);
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "SaveAsync() called — both orders reloaded through a fresh scope",
        //    new[]
        //    {
        //        "stagingRepo.SaveAsync() issued one SaveChangesAsync() — one database transaction.",
        //        "Reloading through a brand-new scope confirms both hits reached the database:",
        //        "",
        //        $"  Order 1 found:  {loaded1 != null}   Id: {loaded1!.Id}   Status: {loaded1.Status}",
        //        $"  Order 2 found:  {loaded2 != null}   Id: {loaded2!.Id}   Status: {loaded2.Status}",
        //        "",
        //        "One write boundary = one transaction = both committed or neither."
        //    }));
        //
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        //
        //// ── PART 3: Explicit boundary — exception before SaveAsync ────────────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Part 3 — Explicit Write Boundary: Exception Before SaveAsync"),
        //    ConsoleColor.DarkBlue);
        //
        //int order3Id, order4Id;
        //string abortReason;
        //
        //using (var abortedScope = serviceProvider.CreateScope())
        //{
        //    var abortedRepo = abortedScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //
        //    var order3 = Order.Place(SeedDataHelper.CustomerAId, new[]
        //    {
        //        (SeedDataHelper.Product3Id, 5, Money.Create(9.99m, "USD"))
        //    });
        //    var order4 = Order.Place(SeedDataHelper.CustomerBId, new[]
        //    {
        //        (SeedDataHelper.Product1Id, 1, Money.Create(29.99m, "USD"))
        //    });
        //
        //    order3Id = order3.Id;
        //    order4Id = order4.Id;
        //
        //    abortedRepo.Add(order3);
        //    abortedRepo.Add(order4);
        //
        //    try
        //    {
        //        // Simulate a use-case failure after staging — SaveAsync() is never reached
        //        throw new InvalidOperationException("Payment validation failed — aborting use case.");
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        abortReason = ex.Message;
        //        // abortedScope disposes here without SaveAsync — no SQL was ever issued
        //    }
        //}
        //
        //// Fresh scope confirms neither order was committed
        //using var confirmNoWriteScope = serviceProvider.CreateScope();
        //var confirmNoWriteRepo = confirmNoWriteScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //var check3             = await confirmNoWriteRepo.GetByIdAsync(order3Id);
        //var check4             = await confirmNoWriteRepo.GetByIdAsync(order4Id);
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "Exception before SaveAsync — confirming zero writes",
        //    new[]
        //    {
        //        "Use case: staged order3 and order4 via Add(), then threw before SaveAsync().",
        //        $"Exception: {abortReason}",
        //        "Scope disposed without SaveAsync — no SQL reached the database.",
        //        "",
        //        $"  Order 3 found:  {check3 != null}    (expected: False)",
        //        $"  Order 4 found:  {check4 != null}    (expected: False)",
        //        "",
        //        "This is the intentional write boundary in action:",
        //        "  The use-case handler owns SaveAsync() — it calls it once, at the end.",
        //        "  Any exception before that point means zero writes — consistent state.",
        //        "  Scatter SaveAsync() after each Add() and this atomicity guarantee is gone."
        //    }));
        //
        //Console.WriteLine();
        //
        //// Silent cleanup of Part 2 orders
        //using var cleanupScope = serviceProvider.CreateScope();
        //var cleanupRepo = cleanupScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //var c1 = await cleanupRepo.GetByIdAsync(order1.Id);
        //var c2 = await cleanupRepo.GetByIdAsync(order2.Id);
        //if (c1 is not null) cleanupRepo.Remove(c1);
        //if (c2 is not null) cleanupRepo.Remove(c2);
        //await cleanupRepo.SaveAsync();

        await Task.CompletedTask;
    }
}
