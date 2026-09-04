using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateOrderAggregate
{
public static async Task RunEndToEndAsync(IServiceProvider serviceProvider)
    {
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Order Aggregate — End to End"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Not Yet Implemented",
            new[]
            {
                "This demo requires ALL of the following clips to be completed first:",
                "  Clip 2  — _lines backing field + IReadOnlyCollection<OrderLine>",
                "  Clip 3  — Private setters",
                "  Clip 4  — Total as Money + OwnsOne mapping + migration",
                "  Clip 5  — CustomerId as shadow property + set before save",
                "  Clip 8  — Guard clauses (Money, OrderLine, Order) + state-transition guards",
                "",
                "When all of the above are complete:",
                "  Open DemonstrateOrderAggregate.cs",
                "  Find the TODO: Module 2 Clip 9 block below this NTI box",
                "  Uncomment the entire block"
            }
        ));

        //TODO: Module 2 Clip 9 — Uncomment ONLY after completing ALL of Clips 2, 3, 4, 5, and 8.
        // WARNING: Uncommenting before those clips are complete WILL crash the demo.
        // (Missing backing field, missing shadow-property set, missing guards, or missing Money
        //  mapping will each throw their own exception before any output is shown.)

        //using (var seedScope = serviceProvider.CreateScope())
        //{
        //    var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
        //    await SeedDataHelper.EnsureCustomersAsync(seedCtx);
        //}
        //
        //// --- PART 1: Create the aggregate via factory ---
        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Order Aggregate — Part 1: Create via Order.Place()"), ConsoleColor.DarkBlue);
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "The aggregate boundary",
        //    new[]
        //    {
        //        "Order is the Aggregate Root.",
        //        "OrderLine is an entity that only exists inside an Order — never standalone.",
        //        "Money is a Value Object owned by OrderLine — mapped inline (no join table).",
        //        "",
        //        "Order.Place() is the only valid entry point.",
        //        "It enforces: at least one line, valid Money, valid quantities.",
        //        "Everything is wired together before the object graph is returned."
        //    }
        //));

        // //----------------------------------------------------------------//
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        // //----------------------------------------------------------------//

        //var order = Order.Place(
        //    SeedDataHelper.CustomerAId,
        //    new[]
        //    {
        //        (SeedDataHelper.Product1Id, 2, Money.Create(29.99m, "USD")),
        //        (SeedDataHelper.Product2Id, 1, Money.Create(49.99m, "USD"))
        //    }
        //);

        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Place the order, show details"), ConsoleColor.DarkBlue);
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "Order.Place() — result",
        //    new[]
        //    {
        //        $"Id:              {order.Id}",
        //        $"Status:          {order.Status}",
        //        $"PlacedAt:        {order.PlacedAt:u}",
        //        $"Lines:           {order.Lines.Count}",
        //        $"Total.Amount:    {order.Total.Amount:C} {order.Total.Currency}",
        //        $"Line[0] Qty:     {order.Lines.First().Quantity}",
        //        $"Line[0] Price:   {order.Lines.First().UnitPrice.Amount:C} {order.Lines.First().UnitPrice.Currency}",
        //        $"Line[1] Qty:     {order.Lines.Last().Quantity}",
        //        $"Line[1] Price:   {order.Lines.Last().UnitPrice.Amount:C} {order.Lines.Last().UnitPrice.Currency}"
        //    }
        //));

        // //----------------------------------------------------------------//
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        // //----------------------------------------------------------------//

        // // --- PART 2: Private collection — Lines is IReadOnlyCollection ---
        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 2: Private Collection — IReadOnlyCollection<OrderLine>"), ConsoleColor.DarkBlue);
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "How the private collection is enforced",
        //    new[]
        //    {
        //        "private readonly List<OrderLine> _lines = new();",
        //        "public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();",
        //        "",
        //        "External callers get a read-only view — they cannot Add, Remove, or Clear.",
        //        "The only mutation path is Order.AddLine(), which applies its own guards.",
        //        "",
        //        "EF Core writes directly to the backing field _lines during materialization.",
        //        "This is configured via: builder.Navigation(o => o.Lines).HasField(\"_lines\")"
        //    }
        //));

        // //----------------------------------------------------------------//
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        // //----------------------------------------------------------------//

        //order.AddLine(SeedDataHelper.Product3Id, 3, Money.Create(9.99m, "USD"));
        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Adding an orderline via order.AddLine"), ConsoleColor.DarkBlue);
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "order.AddLine() — the only valid mutation path",
        //    new[]
        //    {
        //        $"Lines after AddLine: {order.Lines.Count}",
        //        $"New line Qty:        {order.Lines.Last().Quantity}",
        //        $"New line Price:      {order.Lines.Last().UnitPrice.Amount:C} {order.Lines.Last().UnitPrice.Currency}",
        //        $"Order Total.Amount:  {order.Total.Amount:C} {order.Total.Currency}",
        //        "",
        //        "// order.Lines.Add(...) — does not compile: IReadOnlyCollection has no Add()"
        //    }
        //));

        // //----------------------------------------------------------------//
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        // //----------------------------------------------------------------//

        // // --- PART 3: Status invariant — state machine enforced by Order methods ---
        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 3: Status Invariant — Ship then Cancel (should throw)"), ConsoleColor.DarkBlue);

        //order.Process();
        //order.Confirm();
        //order.Ship();

        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "order.Ship() — succeeded",
        //    new[]
        //    {
        //        $"Status is now: {order.Status}",
        //        "",
        //        "Now attempting order.Cancel() on a Shipped order..."
        //    }
        //));

        // //----------------------------------------------------------------//
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        // //----------------------------------------------------------------//

        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Order Cannot be cancelled once shipped"), ConsoleColor.DarkBlue);
        //try
        //{
        //    order.Cancel();
        //}
        //catch (InvalidOperationException ex)
        //{
        //    Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //        "order.Cancel() — InvalidOperationException caught",
        //        new[]
        //        {
        //            $"Exception: {ex.GetType().Name}",
        //            $"Message:   {ex.Message}",
        //            "",
        //            $"Status remains: {order.Status}",
        //            "",
        //            "The aggregate enforces its own state machine.",
        //            "No caller can put the Order into an illegal state transition."
        //        }
        //    ));
        //}

        // //----------------------------------------------------------------//
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        // //----------------------------------------------------------------//

        // // --- PART 4: Persist and reload — full round-trip ---
        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 4: Persist and Reload — Full Round-Trip"), ConsoleColor.DarkBlue);

        //int savedId;

        //using (var scope = serviceProvider.CreateScope())
        //{
        //    var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        //    ctx.Orders.Add(order);
        //    ctx.Entry(order).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
        //    await ctx.SaveChangesAsync();
        //    savedId = order.Id;
        //}

        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "SaveChangesAsync() — persisted",
        //    new[]
        //    {
        //        $"Saved Order Id:  {savedId}",
        //        "",
        //        "Now loading from a completely fresh DbContext scope...",
        //        "If EF materialization is wired correctly:",
        //        "  - _lines backing field will be populated (not empty)",
        //        "  - Status private setter will hold the persisted value",
        //        "  - Money columns will be read back inline (no join)"
        //    }
        //));

        // //----------------------------------------------------------------//
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        // //----------------------------------------------------------------//

        //Order reloaded;
        //int reloadedCustomerId = 0;

        //using (var scope = serviceProvider.CreateScope())
        //{
        //    var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        //    reloaded = await ctx.Orders
        //        .Include(o => o.Lines)
        //        .FirstAsync(o => o.Id == savedId);
        //    reloadedCustomerId = ctx.Entry(reloaded).Property<int>("CustomerId").CurrentValue;
        //}

        //var linesOk  = reloaded.Lines.Count == 3;
        //var statusOk = reloaded.Status == OrderManagement.Domain.Enums.OrderStatus.Shipped;

        //var line1    = reloaded.Lines.FirstOrDefault(l => l.ProductId == SeedDataHelper.Product1Id);
        //var moneyLine1Ok = line1 is not null
        //            && line1.UnitPrice.Amount == 29.99m;
        //var moneyLine1CurrencyOk = line1 is not null
        //            && line1.UnitPrice.Currency == "USD";

        //var line2 = reloaded.Lines.FirstOrDefault(l => l.ProductId == SeedDataHelper.Product2Id);
        //var moneyLine2Ok = line2 is not null
        //            && line2.UnitPrice.Amount == 49.99m;
        //var moneyLine2CurrencyOk = line2 is not null
        //                            && line2.UnitPrice.Currency == "USD";

        //var line3 = reloaded.Lines.FirstOrDefault(l => l.ProductId == SeedDataHelper.Product3Id);
        //var moneyLine3Ok = line3 is not null
        //            && line3.UnitPrice.Amount == 9.99m;
        //var moneyLine3CurrencyOk = line3 is not null
        //                            && line3.UnitPrice.Currency == "USD";
        //var totalAmountOk = reloaded.Total.Amount == 139.94m;
        //var totalCurrencyOk = reloaded.Total.Currency == "USD";

        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Show order details and checks"), ConsoleColor.DarkBlue);
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "Reload assertions — integration verification",
        //    new[]
        //    {
        //        $"Id:                       {reloaded.Id}",
        //        $"CustomerId:               {reloadedCustomerId}  (shadow property)",
        //        $"Status (private setter):  {reloaded.Status}  — expected Shipped  [{(statusOk ? "PASS" : "FAIL")}]",
        //        $"Lines (_lines field):     {reloaded.Lines.Count}  — expected 3       [{(linesOk ? "PASS" : "FAIL")}]",
        //        $"Line[0] Money.Amount:     {line1?.UnitPrice.Amount ?? 0}  — expected 29.99   [{(moneyLine1Ok ? "PASS" : "FAIL")}]",
        //        $"Line[0] Money.Currency:   {line1?.UnitPrice.Currency ?? "?"}  — expected USD     [{(moneyLine1CurrencyOk ? "PASS" : "FAIL")}]",
        //        $"Line[1] Money.Amount:     {line2?.UnitPrice.Amount ?? 0}  — expected 49.99   [{(moneyLine2Ok ? "PASS" : "FAIL")}]",
        //        $"Line[1] Money.Currency:   {line2?.UnitPrice.Currency ?? "?"}  — expected USD     [{(moneyLine2CurrencyOk ? "PASS" : "FAIL")}]",
        //        $"Line[2] Money.Amount:     {line3?.UnitPrice.Amount ?? 0}  — expected 9.99    [{(moneyLine3Ok ? "PASS" : "FAIL")}]",
        //        $"Line[2] Money.Currency:   {line3?.UnitPrice.Currency ?? "?"}  — expected USD     [{(moneyLine3CurrencyOk ? "PASS" : "FAIL")}]",
        //        $"Total.Amount:   {reloaded.Total.Amount}  — expected 139.94 [{(totalAmountOk ? "PASS" : "FAIL")}]",
        //        $"Total.Currency: {reloaded.Total.Currency}  — expected USD [{(totalCurrencyOk ? "PASS" : "FAIL")}]",
        //        "",
        //        "All four EF mapping concerns verified after a full round-trip:",
        //        "  1. Private setter on Status — persisted and restored",
        //        "  2. Backing field _lines      — populated by EF materializer",
        //        "  3. Owned Money (OwnsOne)     — round-tripped inline, no join needed",
        //        "  4. Order Total               — recomputed in domain and persisted by EF"
        //    }
        //));

        // //----------------------------------------------------------------//
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        // //----------------------------------------------------------------//

        // // --- PART 5: The change-tracker bypass vector ---
        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 5: The Bypass Vector — context.Entry(...).Property(...).CurrentValue"), ConsoleColor.DarkBlue);
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "Everything we just saw protects the application layer — not ORM infrastructure",
        //    new[]
        //    {
        //        "The private constructor, private setters, and IReadOnlyCollection all block",
        //        "application code from reaching invalid state.",
        //        "",
        //        "But EF Core's change tracker can bypass every one of those guards:",
        //        "",
        //        "  context.Entry(order).Property(\"Status\").CurrentValue = OrderStatus.Cancelled;",
        //        "  await context.SaveChangesAsync();",
        //        "",
        //        "This compiles. It runs. It writes to the database.",
        //        "No guard fires. No domain event is raised. No state machine is consulted.",
        //        "",
        //        "This is not unique to EF Core — any ORM or raw SQL can do the same thing.",
        //        "The invariant boundary in this module is the application layer, not the DB layer."
        //    }
        //));

        // //----------------------------------------------------------------//
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        // //----------------------------------------------------------------//

        // // Live demo of the bypass
        //using (var scope = serviceProvider.CreateScope())
        //{
        //    var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        //    var target = await ctx.Orders.FirstAsync(o => o.Id == savedId);

        //    ctx.Entry(target).Property("Status").CurrentValue = OrderManagement.Domain.Enums.OrderStatus.Placed;
        //    await ctx.SaveChangesAsync();
        //}

        //Order bypassed;
        //using (var scope = serviceProvider.CreateScope())
        //{
        //    var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        //    bypassed = await ctx.Orders.FirstAsync(o => o.Id == savedId);
        //}

        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Change-Tracker bypass"), ConsoleColor.DarkBlue);
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "After the change-tracker bypass",
        //    new[]
        //    {
        //        $"Status is now: {bypassed.Status}  (was Shipped — reverted to Placed without calling any method)",
        //        "",
        //        "No exception. No guard. No domain event.",
        //        "The change tracker wrote directly past every invariant."
        //    }
        //));

        // //----------------------------------------------------------------//
        //Console.WriteLine();
        //InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //Console.WriteLine();
        // //----------------------------------------------------------------//

        // // --- PART 6: Closing discussion — where to add the harder boundary ---
        //OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 6: Adding a Harder Boundary — EF Core Interceptors"), ConsoleColor.DarkBlue);
        //Console.Write(OutputHelpers.BoxedArrayWithTitle(
        //    "If you need to close the change-tracker bypass, use a SavingChangesInterceptor",
        //    new[]
        //    {
        //        "EF Core provides SaveChangesInterceptor with a SavingChangesAsync hook.",
        //        "It runs inside the SaveChanges pipeline — before any SQL is sent.",
        //        "",
        //        "Pattern:",
        //        "  public class DomainInvariantInterceptor : SaveChangesInterceptor",
        //        "  {",
        //        "      public override ValueTask<InterceptionResult<int>> SavingChangesAsync(...)",
        //        "      {",
        //        "          foreach (var entry in context.ChangeTracker.Entries<Order>())",
        //        "              ValidateOrderInvariants(entry.Entity);",
        //        "          return base.SavingChangesAsync(...);",
        //        "      }",
        //        "  }",
        //        "",
        //        "Registered via: optionsBuilder.AddInterceptors(new DomainInvariantInterceptor())",
        //        "",
        //        "This gives you a pre-save validation hook that catches bypass attempts.",
        //        "Use it when the risk of infrastructure-layer mutations is non-trivial.",
        //        "Note: Using a SavingChangesInterceptor is covered later in the course during Module 5: Domain Events Inside the SaveChanges Transaction"
        //    }
        //));

        await Task.CompletedTask;
    }

}
