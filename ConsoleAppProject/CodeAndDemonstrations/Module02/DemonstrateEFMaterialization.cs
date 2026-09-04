using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateEFMaterialization
{
    public static async Task ShowMaterializationWithProof(IServiceProvider serviceProvider)
    {
        // --- PART 1: What the domain model looks like after creation via factory ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("EF Materialization — Part 1: Creating via factory method"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Order.Place(...) — created via factory",
            new[]
            {
                "Order.Place() is the only public way to create an Order.",
                "It sets all core properties, adds lines via AddLine(), and raises a domain event.",
                "Except Total - which is handled soon.",
                "",
                "The private Order() {} constructor exists to allow EF Core materialization.",
                "That private constructor does NOT raise events, and it does NOT add order lines.",
                "Application code cannot call it directly. Private blocks external callers (CS0122)."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // Create via factory and show domain events were raised

        // Uncomment the line below to prove you cannot instantiate Order directly:
        //var cantDoThis = new Order();   // CS0122: 'Order.Order()' is inaccessible due to its protection level

        using (var seedScope = serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.EnsureCustomersAsync(seedCtx);
        }

        var factoryOrder = Order.Place(
            SeedDataHelper.CustomerAId,
            new[]
            {
                (SeedDataHelper.Product1Id, 2, Money.Create(19.99m, "USD"))
            }
        );

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Factory-created order state"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Order created via Order.Place(...)",
            new[]
            {
                $"Id:                  {factoryOrder.Id}",
                $"Status:              {factoryOrder.Status}",
                $"Lines.Count:         {factoryOrder.Lines.Count}   <-- populated via AddLine() inside Place()",
                $"DomainEvents.Count:  {factoryOrder.DomainEvents.Count}   <-- OrderPlacedEvent was raised",
                $"DomainEvents[0]:     {factoryOrder.DomainEvents[0].GetType().Name}"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 2: Persist and reload — EF materializes, bypassing the factory ---
        int savedId;

        using (var scope1 = serviceProvider.CreateScope())
        {
            var ctx1 = scope1.ServiceProvider.GetRequiredService<OrderingContext>();
            ctx1.Orders.Add(factoryOrder);
            ctx1.Entry(factoryOrder).Property<int>("CustomerId").CurrentValue = SeedDataHelper.CustomerAId;
            await ctx1.SaveChangesAsync();
            savedId = factoryOrder.Id;

            OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Order saved — starting fresh DbContext scope"), ConsoleColor.DarkBlue);
            Console.Write(OutputHelpers.BoxedMessage(
                $"Saved Order Id: {savedId}  — now loading in a NEW context scope",
                '*'
            ));
        }

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 3: EF reloads — prove factory was NOT called ---
        using (var scope2 = serviceProvider.CreateScope())
        {
            var ctx2 = scope2.ServiceProvider.GetRequiredService<OrderingContext>();

            var reloaded = await ctx2.Orders
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == savedId);

            OutputHelpers.WriteColored(OutputHelpers.SectionBanner("EF Materialization — Part 3: Reloaded from database"), ConsoleColor.DarkBlue);
            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "Reloaded order — materialized by EF Core",
                new[]
                {
                    $"Id:                  {reloaded!.Id}",
                    $"Status:              {reloaded.Status}",
                    $"Lines.Count:         {reloaded.Lines.Count}   <-- populated, yet Lines has no setter",
                    "Lines.Count > 0 proves EF wrote directly into the private _lines field.",
                    "  Lines is IReadOnlyCollection with no setter — no other path exists.",
                    $"DomainEvents.Count:  {reloaded.DomainEvents.Count}   <-- ZERO: Place() was never called",
                    "",
                    "DomainEvents.Count == 0 proves Order.Place() was never called.",
                    "  EF used the private Order() {} constructor instead.",
                    "  That constructor is an empty shell — no events, no validation."
                }
            ));
        }

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 4: Why private here, and when to use protected? ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("EF Materialization — Part 4: Constructor accessibility"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Why private Order() {} here, and when protected makes sense",
            new[]
            {
                "EF Core requires a constructor it can invoke during materialization.",
                "It prefers a parameterless constructor and can use non-public constructors.",
                "",
                "private Order() {}  — best when this aggregate is not inherited and",
                "                      application code should never call a constructor directly.",
                "",
                "protected Order() {} — use when inheritance/proxies are required and",
                "                        EF still needs non-public construction support.",
                "",
                "Either way: your factory method Order.Place() remains the only",
                "legitimate entry point for application code."
            }
        ));
    }
}
