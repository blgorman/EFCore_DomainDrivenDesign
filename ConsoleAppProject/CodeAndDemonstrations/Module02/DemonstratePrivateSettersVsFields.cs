using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstratePrivateSettersVsFields
{
    public static async Task CompareApproachesAsync(IServiceProvider serviceProvider)
    {
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Scenario A - Private Setter: Status {get; private set; }"), ConsoleColor.DarkBlue);

        await ScenarioAPrivateSetterAsync(serviceProvider);

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Scenario B — Private Backing Field: _lines via Lines property"), ConsoleColor.DarkBlue);

        await ScenarioBBackingFieldAsync(serviceProvider);
    }

    private static async Task ScenarioAPrivateSetterAsync(IServiceProvider serviceProvider)
    {
        using var scopeA = serviceProvider.CreateScope();
        var ctxA = scopeA.ServiceProvider.GetRequiredService<OrderingContext>();
        await SeedDataHelper.EnsureCustomersAsync(ctxA);

        //place an order and ship it, which changes the Status property through the Ship() domain method
        /*
        The key point this is proving: EF is just a persistence mechanism. 
        The order's state is valid and complete entirely within the domain before EF ever sees it. 
        You don't need to save-then-update, or rely on EF to orchestrate the state transitions.
        */
        var order = Order.Place(
            SeedDataHelper.CustomerAId,
            new[] { (SeedDataHelper.Product1Id, 1, Money.Create(15m, "USD")) }
        );

        //TODO: Module 2 Clip 3 — After making Status private set, comment this line out (it will be a compile error).
        order.Status = OrderStatus.Shipped; // Status is currently public set — external code can bypass domain methods
        order.Process();
        order.Confirm();
        order.Ship();

        //show the order before saving:
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Order state before saving"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Scenario A — Order State BEFORE SaveChangesAsync",
            new[]
            {
                $"Order Id:   {order.Id}",
                $"Status:     {order.Status}",
                $"PlacedAt:   {order.PlacedAt:u}",
                "",
                "State is fully set by domain methods — EF has not been called yet.",
                "CustomerId will become a shadow property in Clip 5.",
                //TODO: Module 2 Clip 3 — After making Status private set, uncomment these two lines and delete the placeholder below:
                //"order.Status = OrderStatus.Shipped — that line above is now commented out (private set is a compile error).",
                //"Status changes only through domain methods: Process, Confirm, Ship, Cancel.",
                "[TODO Clip 3] Status is public set — make it private, then uncomment lines above and delete this one."
            }
        ));

        //-----------------------------------------------------------------
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //-----------------------------------------------------------------

        //add (persist) the order.
        ctxA.Orders.Add(order);
        //TODO: Module 2 Clip 5 — After making CustomerId a shadow property, set it here before saving:
        //ctxA.Entry(order).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
        await ctxA.SaveChangesAsync();

        //get the id of the saved order to demonstrate that EF persisted and reloaded the Status value correctly, even though it has a private setter.
        var savedId = order.Id;


        /*
            The reason it's needed: EF's identity map (change tracker) caches entities by primary key within a single context instance. 
            If you reloaded the order using ctxA (the same context that just saved it), EF would return the already-tracked, 
                in-memory object rather than actually hitting the database. The round-trip would be fake.

            By creating scopeA2 with its own ctxA2, you get a context that has never seen this order — 
                so FindAsync(savedId) genuinely queries the database and materializes a fresh object. 
                That's what proves EF correctly persisted and can restore the value. 
        */
        using var scopeA2 = serviceProvider.CreateScope();
        var ctxA2 = scopeA2.ServiceProvider.GetRequiredService<OrderingContext>();

        //reload the order from the database to show that EF correctly persisted and restored the Status value through the private setter.
        var reloaded = await ctxA2.Orders.FindAsync(savedId);

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Order as retrieved from database"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Scenario A — EF Round-Trip (private setter)",
            new[]
            {
                $"Order Id:            {order.Id}",
                $"CustomerId:          {ctxA2.Entry(reloaded!).Property<int>("CustomerId").CurrentValue}",
                $"Status after Ship():  {order.Status}",
                $"PlacedAt:   {order.PlacedAt:u}",
                $"Reloaded Status:      {reloaded!.Status}",
                "  -> EF persisted and restored the value through the private setter.",
                //TODO: Module 2 Clip 3 — After making Status private set, uncomment the line below and delete the placeholder:
                //"  -> order.Status = ... direct assignment is now a compile error (private set enforced).",
                "  -> [TODO Clip 3] Make Status private set; then uncomment line above and delete this one.",
                "  -> Only domain methods (Ship, Cancel) can change Status — invariant enforced."
            }
        ));

        //-----------------------------------------------------------------
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //-----------------------------------------------------------------
    }

    private static async Task ScenarioBBackingFieldAsync(IServiceProvider serviceProvider)
    {
        using var scopeB = serviceProvider.CreateScope();
        var ctxB = scopeB.ServiceProvider.GetRequiredService<OrderingContext>();

        var order = Order.Place(
            SeedDataHelper.CustomerAId,
            new[] { (SeedDataHelper.Product1Id, 2, Money.Create(20m, "USD")) }
        );

        var lineCountBefore = order.Lines.Count;
        // AddLine writes to the private _lines field — not accessible directly from here
        //order.Lines.Add(new OrderLine(SeedDataHelper.Product1Id, 2, Money.Create(20m, "USD")));
        order.AddLine(SeedDataHelper.Product2Id, 1, Money.Create(50m, "USD"));
        var lineCountAfter = order.Lines.Count;

        // show the state of the order with lines before persisting:
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Use the AddLine method to add order lines to the private backing field"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Scenario B — Adding lines via AddLine() method",
            new[]
            {
                $"Lines count before AddLine(): {lineCountBefore}",
                $"Lines count after AddLine():  {lineCountAfter}",
                "  -> The public Lines property returns IReadOnlyCollection<OrderLine>.",
                "  -> Callers cannot call Lines.Add(...) — encapsulation enforced at compile time."
            }
        ));  

        //-----------------------------------------------------------------
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //-----------------------------------------------------------------

        ctxB.Orders.Add(order);
        //TODO: Module 2 Clip 5 — After making CustomerId a shadow property, set it here before saving:
        //ctxB.Entry(order).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
        await ctxB.SaveChangesAsync();

        var savedId = order.Id;

        using var scopeB2 = serviceProvider.CreateScope();
        var ctxB2 = scopeB2.ServiceProvider.GetRequiredService<OrderingContext>();

        var reloaded = await ctxB2.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == savedId);

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Order as retrieved from database"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Scenario B — EF Round-Trip (private backing field _lines)",
            new[]
            {
                $"Lines added via AddLine(): {order.Lines.Count}",
                $"Reloaded Lines count:      {reloaded!.Lines.Count}",
                "  -> EF wrote to '_lines' directly during materialization.",
                "  -> The public 'Lines' property returns IReadOnlyCollection<OrderLine>.",
                "  -> Callers cannot call Lines.Add(...) — encapsulation enforced at compile time."
            }
        ));
    }
}
