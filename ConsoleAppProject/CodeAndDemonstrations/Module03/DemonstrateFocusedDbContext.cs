using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateFocusedDbContext
{
    public static async Task ShowOrderingContextAsync(IServiceProvider serviceProvider)
    {
        // --- Screen 1: What is a focused DbContext? ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Designing a Focused DbContext"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "What does it mean to 'scope' a DbContext to one business capability?",
            new[]
            {
                "A focused DbContext only knows about the entities its bounded context owns.",
                "",
                "It has NO DbSet<T> for entities that belong to another context.",
                "It has NO navigation properties that cross a context boundary.",
                "It has NO model configuration for types it doesn't own.",
                "",
                "The types absent from the context are absent BY DESIGN — not by omission.",
                "",
                "OrderingContext  -->  owns Order, OrderLine, Customer",
                "ShippingContext  -->  owns Shipment"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 2: OrderingContext entity types from the EF model ---
        using var orderingScope = serviceProvider.CreateScope();
        var orderingCtx = orderingScope.ServiceProvider.GetRequiredService<OrderingContext>();

        var orderingTypes = orderingCtx.Model
            .GetEntityTypes()
            .Select(e => e.ClrType.Name)
            .OrderBy(n => n)
            .ToArray();

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("OrderingContext — Registered Entity Types"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            $"orderingCtx.Model.GetEntityTypes()  — {orderingTypes.Length} types",
            orderingTypes
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 3: Walk the OrderingContext DbSet properties and explain absent types ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("OrderingContext — DbSet Properties and What Is Deliberately Absent"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderingContext — What Is Registered and Why",
            new[]
            {
                "REGISTERED (DbSet<T> declared on this context):",
                "  DbSet<Order>     Orders     — the aggregate root the Ordering domain writes",
                "  DbSet<Customer>  Customers  — the customer who placed the order",
                "  OrderLine                   — registered via OrderConfiguration.OwnsMany",
                "",
                "DELIBERATELY ABSENT (no DbSet<T>, no model config):",
                "  Shipment  — owned by ShippingContext; Ordering has no business writing it",
                "  Payment   — belongs to a Payments bounded context (not shown here)",
                "  Inventory — belongs to a Catalogue/Inventory context",
                "",
                "If OrderingContext.Shipments existed it would couple two bounded contexts.",
                "The absence of Shipments here is a deliberate architecture decision.",
                "ShippingContext manages that data independently."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//


        // --- Screen 4: ShippingContext entity types from the EF model ---
        using var shippingScope = serviceProvider.CreateScope();
        var shippingCtx = shippingScope.ServiceProvider.GetRequiredService<ShippingContext>();

        var shippingTypes = shippingCtx.Model
            .GetEntityTypes()
            .Select(e => e.ClrType.Name)
            .OrderBy(n => n)
            .ToArray();

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("ShippingContext — Registered Entity Types"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            $"shippingCtx.Model.GetEntityTypes()  — {shippingTypes.Length} type",
            shippingTypes
        ));

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "ShippingContext — What Is Registered and What Is Deliberately Absent",
            new[]
            {
                "REGISTERED (DbSet<T> declared on this context):",
                "  DbSet<ShipmentReadModel>  Shipments  — the only type Shipping owns",
                "",
                "DELIBERATELY ABSENT:",
                "  Order, OrderLine, Customer — Shipping has no business writing these.",
                "  No CustomerId, no Lines collection, no Money value object here.",
                "",
                "ShippingContext cannot accidentally query or update Order-domain data.",
                "It is isolated from the Ordering bounded context by design."
            }
        ));

        await Task.CompletedTask;
    }
}
