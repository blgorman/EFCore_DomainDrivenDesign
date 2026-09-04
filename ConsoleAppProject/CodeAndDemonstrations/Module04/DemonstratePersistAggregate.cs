using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstratePersistAggregate
{
    public static async Task ShowWriteSideAsync(IServiceProvider serviceProvider)
    {
        //TODO: Module 4 Clip 6 — Delete the "Not Yet Implemented" box below.
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This demo will work after completing Module 4 Clip 6.",
                "Open DemonstratePersistAggregate.cs and uncomment the //TODO: Module 4 Clip 6 block.",
                "Prerequisite: OrderRepository must already implement Add, Remove, SaveAsync, and GetByIdAsync (Module 4 Clip 4)."
            }
        ));

        //TODO: Module 4 Clip 6 — Uncomment the write-side walkthrough below.
        //// Seed in its own scope so demonstration scopes start with a clean ChangeTracker
        //using (var seedScope = serviceProvider.CreateScope())
        //{
        //    var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
        //    await SeedDataHelper.SeedAsync(seedCtx);
        //}
        //
        //// ── PART 1: Add() — tracking without flushing ─────────────────────────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Part 1 — Add(): Tracking Without Flushing to the Database"),
        //    ConsoleColor.DarkBlue);
        //
        //using var writeScope = serviceProvider.CreateScope();
        //var writeCtx  = writeScope.ServiceProvider.GetRequiredService<OrderingContext>();
        //var writeRepo = writeScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //
        //var newOrder = Order.Place(SeedDataHelper.CustomerAId, new[]
        //{
        //    (SeedDataHelper.Product1Id, 2, Money.Create(29.99m, "USD")),
        //    (SeedDataHelper.Product2Id, 1, Money.Create(49.99m, "USD"))
        //});
        //
        //// The overload sets the CustomerId shadow FK, which Order.Place() never sets.
        //writeRepo.Add(newOrder, SeedDataHelper.CustomerAId);
        //
        //var addedEntry = writeCtx.ChangeTracker.Entries<Order>()
        //    .First(e => e.Entity.Id == newOrder.Id);
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "repository.Add(newOrder, customerId) — ChangeTracker state before SaveAsync",
        //    new[]
        //    {
        //        "repository.Add(newOrder, CustomerAId) called. No SaveAsync() has been called yet.",
        //        "",
        //        $"  ChangeTracker.Entries<Order>().Count():  {writeCtx.ChangeTracker.Entries<Order>().Count()}",
        //        $"  EntityState for new order:               {addedEntry.State}",
        //        "",
        //        "EntityState.Added — EF is tracking the order but has issued no SQL.",
        //        "The INSERT will not reach the database until SaveAsync() is called.",
        //        "Add() is purely an in-memory operation on the ChangeTracker."
        //    }));
        //
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        //
        //// ── PART 2: SaveAsync() — single flush, then reload to confirm ─────────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Part 2 — SaveAsync(): Flushing the Unit of Work"),
        //    ConsoleColor.DarkBlue);
        //
        //await writeRepo.SaveAsync();
        //
        //// Fresh scope — proves the data truly hit the database, not just the ChangeTracker
        //using var readBackScope = serviceProvider.CreateScope();
        //var readBackCtx  = readBackScope.ServiceProvider.GetRequiredService<OrderingContext>();
        //var readBackRepo = readBackScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //var reloaded     = await readBackRepo.GetByIdAsync(newOrder.Id);
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "SaveAsync() called — reload through a fresh scope to confirm persistence",
        //    new[]
        //    {
        //        "writeRepo.SaveAsync() issued one SaveChangesAsync() — one database transaction.",
        //        "Loading the same order through a brand-new repository scope:",
        //        "",
        //        $"  Order found:   {reloaded != null}",
        //        $"  Id:            {reloaded!.Id}",
        //        $"  CustomerId:    {readBackCtx.Entry(reloaded).Property<int>("CustomerId").CurrentValue}",
        //        $"  Status:        {reloaded.Status}",
        //        $"  Lines loaded:  {reloaded.Lines.Count}  (full aggregate)",
        //        "",
        //        "One SaveAsync() = one atomic transaction covering all staged changes."
        //    }));
        //
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        //
        //// ── PART 3: Remove() — mark for deletion, then confirm ────────────────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Part 3 — Remove(): Marking the Aggregate for Deletion"),
        //    ConsoleColor.DarkBlue);
        //
        //using var deleteScope = serviceProvider.CreateScope();
        //var deleteCtx  = deleteScope.ServiceProvider.GetRequiredService<OrderingContext>();
        //var deleteRepo = deleteScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //
        //var toDelete = await deleteRepo.GetByIdAsync(newOrder.Id);
        //deleteRepo.Remove(toDelete!);
        //
        //// Capture state before SaveAsync — proves Remove() only marks, does not flush
        //var deletedState = deleteCtx.ChangeTracker.Entries<Order>()
        //    .First(e => e.Entity.Id == newOrder.Id).State;
        //
        //await deleteRepo.SaveAsync();
        //
        //using var confirmDeleteScope = serviceProvider.CreateScope();
        //var confirmDeleteRepo = confirmDeleteScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //var afterDelete       = await confirmDeleteRepo.GetByIdAsync(newOrder.Id);
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "repository.Remove(order) — ChangeTracker state, then SaveAsync confirmation",
        //    new[]
        //    {
        //        "repository.Remove(toDelete) called. ChangeTracker state before SaveAsync:",
        //        $"  EntityState:  {deletedState}",
        //        "  EntityState.Deleted — EF is tracking the deletion but has issued no SQL yet.",
        //        "---",
        //        "repository.SaveAsync() called — DELETE issued:",
        //        $"  GetByIdAsync({newOrder.Id})  =>  " +
        //            $"{(afterDelete == null ? "null — DELETE confirmed, aggregate is gone" : $"still found: {afterDelete.Id}")}",
        //    }));
        //
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        //
        //// ── PART 4: Why SaveAsync belongs on the repository ───────────────────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Part 4 — Why SaveAsync Belongs on the Repository"),
        //    ConsoleColor.DarkBlue);
        //
        //var interfaceMethods = typeof(IOrderRepository)
        //    .GetMethods()
        //    .Select(m => $"  {m.Name}")
        //    .OrderBy(s => s)
        //    .ToArray();
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "IOrderRepository — the only surface callers can reach",
        //    interfaceMethods
        //        .Concat(new[]
        //        {
        //            "---",
        //            "SaveAsync() is the only flush path on this interface.",
        //            "Callers hold IOrderRepository (a domain type) — OrderingContext is invisible to them.",
        //            "SaveChangesAsync() does not appear on IOrderRepository.",
        //            "---",
        //            "If callers held OrderingContext directly, they could scatter SaveChangesAsync():",
        //            "  After every Add()   — N INSERT round trips instead of 1",
        //            "  Mid-use-case        — flushes partial state before the operation completes",
        //            "  Inside a loop       — breaks atomicity across iterations",
        //            "---",
        //            "The repository enforces one SaveAsync() per use case = one atomic transaction."
        //        })
        //        .ToArray()));
        //
        //Console.WriteLine();

        await Task.CompletedTask;
    }
}
