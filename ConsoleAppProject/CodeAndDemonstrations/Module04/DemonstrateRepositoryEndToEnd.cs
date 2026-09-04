using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Commands;
using OrderManagement.Application.Handlers;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateRepositoryEndToEnd
{
    public static async Task RunEndToEndAsync(IServiceProvider serviceProvider)
    {
        //TODO: Module 4 Clip 9 — Delete the "Not Yet Implemented" box below.
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This demo will work after completing Module 4 Clip 9.",
                "Open DemonstrateRepositoryEndToEnd.cs and uncomment the //TODO: Module 4 Clip 9 block.",
                "Prerequisite: OrderRepository must already implement Add, SaveAsync, GetByIdAsync, and Remove (Module 4 Clip 4)."
            }
        ));

        //TODO: Module 4 Clip 9 — Uncomment the end-to-end walkthrough below.
        //// Seed so CustomerAId, Product IDs, etc. exist
        //using (var seedScope = serviceProvider.CreateScope())
        //{
        //    var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
        //    await SeedDataHelper.SeedAsync(seedCtx);
        //}
        //
        //int orderId;
        //
        //// ── STEP 1: Create the aggregate in memory ─────────────────────────────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Step 1 — Order.Place(): Creating the Aggregate in Memory"),
        //    ConsoleColor.DarkBlue);
        //
        //using var writeScope = serviceProvider.CreateScope();
        //var writeRepo = writeScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //
        //var order = Order.Place(SeedDataHelper.CustomerAId, new[]
        //{
        //    (SeedDataHelper.Product1Id, 2, Money.Create(29.99m, "USD")),
        //    (SeedDataHelper.Product2Id, 1, Money.Create(49.99m, "USD"))
        //});
        //
        //var initialLines = order.Lines
        //    .Select((l, i) =>
        //        $"  Line {i + 1}: ProductId={l.ProductId}  " +
        //        $"Qty={l.Quantity}   UnitPrice={l.UnitPrice.Amount:F2} {l.UnitPrice.Currency}")
        //    .ToArray();
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "Order created in memory — not yet persisted",
        //    new[]
        //    {
        //        $"Id:       {order.Id}",
        //        $"Status:   {order.Status}",
        //        $"Lines:    {order.Lines.Count}  (in-memory only — no INSERT issued yet)",
        //        "---"
        //    }
        //    .Concat(initialLines)
        //    .ToArray()));
        //
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        //
        //// ── STEP 2: Persist via repository ────────────────────────────────────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Step 2 — repository.Add() + repository.SaveAsync(): Persist to the Database"),
        //    ConsoleColor.DarkBlue);
        //
        //// The overload sets the CustomerId shadow FK, which Order.Place() never sets.
        //writeRepo.Add(order, SeedDataHelper.CustomerAId);
        //await writeRepo.SaveAsync();
        //orderId = order.Id;
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "Persisted via IOrderRepository — one SaveAsync(), one transaction",
        //    new[]
        //    {
        //        "writeRepo.Add(order, custId)  — stages the order and sets the CustomerId shadow FK",
        //        "writeRepo.SaveAsync()         — flushes all staged changes in one transaction",
        //        "",
        //        $"  Generated Id:  {orderId}",
        //        "",
        //        "IOrderRepository is defined in Domain (zero EF Core references).",
        //        "OrderRepository is implemented in Infrastructure (wraps OrderingContext).",
        //        "services.AddScoped<IOrderRepository, OrderRepository>() wires them together."
        //    }));
        //
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        //
        //// ── STEP 3: Load by ID through the repository ─────────────────────────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Step 3 — repository.GetByIdAsync(): Loading the Full Aggregate"),
        //    ConsoleColor.DarkBlue);
        //
        //using var updateScope = serviceProvider.CreateScope();
        //var updateCtx  = updateScope.ServiceProvider.GetRequiredService<OrderingContext>();
        //var updateRepo = updateScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //
        //var loaded = await updateRepo.GetByIdAsync(orderId);
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "GetByIdAsync() — full aggregate loaded in a fresh scope",
        //    new[]
        //    {
        //        $"Id:         {loaded!.Id}",
        //        $"Status:     {loaded.Status}",
        //        $"Lines:      {loaded.Lines.Count}  (Include strategy baked into GetByIdAsync)",
        //        "",
        //        "GetByIdAsync always calls .Include(o => o.Lines) internally.",
        //        "The full aggregate is available — business rules can safely inspect Lines."
        //    }));
        //
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        //
        //// ── STEP 4: Mutate aggregate + re-persist ─────────────────────────────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Step 4 — order.AddLine() + repository.SaveAsync(): Write-Side Update"),
        //    ConsoleColor.DarkBlue);
        //
        //var linesBeforeAdd = loaded.Lines.Count;
        //
        //loaded.AddLine(SeedDataHelper.Product3Id, 4, Money.Create(9.99m, "USD"));
        //
        //// DetectChanges does not initialize the shadow FK (OrderId) for newly-detected
        //// child entities. Explicitly registering the new line fixes up all shadow FKs
        //// before SaveChanges issues the INSERT.
        //updateCtx.Add(loaded.Lines.Last());
        //
        //await updateRepo.SaveAsync();
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "AddLine() + SaveAsync() — aggregate mutated and re-persisted",
        //    new[]
        //    {
        //        "loaded.AddLine(Product3Id, 4, Money.Create(9.99m, \"USD\"))",
        //        "updateCtx.Add(loaded.Lines.Last())   // initializes shadow FK before SaveAsync",
        //        "updateRepo.SaveAsync()               // single flush for the mutation use-case",
        //        "",
        //        $"  Lines before AddLine:  {linesBeforeAdd}",
        //        $"  Lines after AddLine:   {loaded.Lines.Count}",
        //        "",
        //        "AddLine() goes through the aggregate root — invariants are enforced.",
        //        "SaveAsync() is the single flush point for the entire mutation use-case."
        //    }));
        //
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        //
        //// ── STEP 5: Reload and print the complete final aggregate ──────────────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Step 5 — Final State: Complete Aggregate Graph"),
        //    ConsoleColor.DarkBlue);
        //
        //using var finalReadScope = serviceProvider.CreateScope();
        //var finalReadRepo = finalReadScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //
        //var complete = await finalReadRepo.GetByIdAsync(orderId);
        //
        //var finalLines = complete!.Lines
        //    .Select((l, i) =>
        //        $"  Line {i + 1}: ProductId={l.ProductId}  " +
        //        $"Qty={l.Quantity}   UnitPrice={l.UnitPrice.Amount:F2} {l.UnitPrice.Currency}")
        //    .ToArray();
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "Complete aggregate — reloaded through a fresh scope",
        //    new[]
        //    {
        //        $"Id:         {complete.Id}",
        //        $"Status:     {complete.Status}",
        //        $"Lines:      {complete.Lines.Count}",
        //        "---"
        //    }
        //    .Concat(finalLines)
        //    .ToArray()));
        //
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        //
        //// ── STEP 6: The same repository, driven by the application layer ───────
        //OutputHelpers.WriteColored(
        //    OutputHelpers.SectionBanner("Step 6 — PlaceOrderHandler: The Caller a Real Application Uses"),
        //    ConsoleColor.DarkBlue);
        //
        //using var handlerScope = serviceProvider.CreateScope();
        //var handler = handlerScope.ServiceProvider.GetRequiredService<PlaceOrderHandler>();
        //
        //var command = new PlaceOrderCommand(
        //    SeedDataHelper.CustomerAId,
        //    new[]
        //    {
        //        new OrderLineInput(SeedDataHelper.Product2Id, 3, 49.99m, "USD"),
        //        new OrderLineInput(SeedDataHelper.Product3Id, 1, 9.99m, "USD")
        //    });
        //
        //var handlerOrderId = await handler.HandleAsync(command);
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "PlaceOrderHandler.HandleAsync — one call, no domain or EF types in sight",
        //    new[]
        //    {
        //        "var command = new PlaceOrderCommand(customerId, new[] { new OrderLineInput(...) });",
        //        "var newId   = await handler.HandleAsync(command);",
        //        "",
        //        $"Returned Id: {handlerOrderId}",
        //        "",
        //        "The command carries int, decimal, and string. Nothing else.",
        //        "This caller never names Order, Money, IOrderRepository, or OrderingContext.",
        //        "OrdersController in OrderManagement.Api calls the handler exactly this way."
        //    }));
        //
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        //
        //// Reload the handler's order through the repository to prove it really persisted
        //using var verifyScope = serviceProvider.CreateScope();
        //var verifyRepo = verifyScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //var handlerOrder = await verifyRepo.GetByIdAsync(handlerOrderId);
        //
        //var handlerLines = handlerOrder!.Lines
        //    .Select((l, i) =>
        //        $"  Line {i + 1}: ProductId={l.ProductId}  " +
        //        $"Qty={l.Quantity}   UnitPrice={l.UnitPrice.Amount:F2} {l.UnitPrice.Currency}")
        //    .ToArray();
        //
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "Reloaded through IOrderRepository — the handler really wrote it",
        //    new[]
        //    {
        //        $"Id:         {handlerOrder.Id}",
        //        $"Status:     {handlerOrder.Status}",
        //        $"Lines:      {handlerOrder.Lines.Count}",
        //        "---"
        //    }
        //    .Concat(handlerLines)
        //    .ToArray()));
        //
        //Console.WriteLine();
        //
        //// Silent cleanup — remove both demo orders
        //using var cleanupScope = serviceProvider.CreateScope();
        //var cleanupRepo = cleanupScope.ServiceProvider.GetRequiredService<IOrderRepository>();
        //foreach (var idToClean in new[] { orderId, handlerOrderId })
        //{
        //    var toClean = await cleanupRepo.GetByIdAsync(idToClean);
        //    if (toClean is not null)
        //    {
        //        cleanupRepo.Remove(toClean);
        //        await cleanupRepo.SaveAsync();
        //    }
        //}

        await Task.CompletedTask;
    }
}
