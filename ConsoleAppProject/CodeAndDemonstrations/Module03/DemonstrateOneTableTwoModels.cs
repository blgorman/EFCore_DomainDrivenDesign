using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateOneTableTwoModels
{
    public static async Task ShowBothContextsAsync(IServiceProvider serviceProvider)
    {
        // --- Screen 1: The concept ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("One Table, Two Models"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "The Orders table exists once in the database — two contexts map it to different CLR types",
            new[]
            {
                "The Orders table is a single SQL table: one schema, one set of rows.",
                "",
                "OrderingContext maps it to:   Order   (rich aggregate)",
                "  private setters, _lines backing field, IReadOnlyCollection<OrderLine>",
                "  UnitPrice mapped as an owned Money value object",
                "  domain behavior: Place(), AddLine(), Ship(), Cancel()",
                "",
                "ShippingContext maps it to:   ShipmentReadModel   (lean read type)",
                "  3 public properties — Id, Status, PlacedAt",
                "  no navigation properties, no domain logic, no value objects",
                "",
                "EF Core handles both via independent model snapshots.",
                "Each DbContext compiles its own model. Neither knows the other exists."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 2: OrderConfiguration — how OrderingContext maps the Orders table ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("OrderingContext — OrderConfiguration"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "How OrderConfiguration maps the Orders table to a rich aggregate",
            new[]
            {
                "builder.HasKey(o => o.Id);",
                "builder.Property(o => o.CustomerId).IsRequired();",
                "builder.Property(o => o.Status).IsRequired();",
                "builder.Property(o => o.PlacedAt).IsRequired();",
                "",
                "// Private backing field — EF writes directly to _lines, no public setter",
                "builder.Navigation(o => o.Lines).HasField(\"_lines\");",
                "builder.HasMany(o => o.Lines)",
                "       .WithOne()",
                "       .HasForeignKey(\"OrderId\")",
                "       .IsRequired();",
                "",
                "// OrderLines table (via OrderLineConfiguration):",
                "//   Id, OrderId (FK), ProductId, Quantity,",
                "//   UnitPrice_Amount, UnitPrice_Currency   (owned Money)"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 3: ShipmentReadModelConfiguration — how ShippingContext maps the same table ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("ShippingContext — ShipmentReadModelConfiguration"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "How ShipmentReadModelConfiguration maps the SAME Orders table to a lean read type",
            new[]
            {
                "// ExcludeFromMigrations() — ShippingContext never owns the Orders schema",
                "builder.ToTable(\"Orders\", t => t.ExcludeFromMigrations());",
                "",
                "builder.HasKey(s => s.Id);",
                "builder.Property(s => s.Status).IsRequired();",
                "builder.Property(s => s.PlacedAt).IsRequired();",
                "",
                "// That's it. No CustomerId. No navigation to OrderLines.",
                "// No Money value object. ShippingContext reads only what it needs.",
                "",
                "Columns visible to ShippingContext from the Orders table:",
                "  Id       (PK)",
                "  Status",
                "  PlacedAt"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 4: Side-by-side column comparison ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("What Each Context Sees From the Orders Table"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderingContext — full column picture",
            new[]
            {
                "Table: Orders",
                "  Id            (PK)",
                "  CustomerId    (required)",
                "  Status        (required)",
                "  PlacedAt      (required)",
                "",
                "Related table: OrderLines",
                "  Id            (PK)",
                "  OrderId       (FK -> Orders.Id)",
                "  ProductId",
                "  Quantity",
                "  UnitPrice_Amount",
                "  UnitPrice_Currency"
            }
        ));

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "ShippingContext — only what Shipping declared",
            new[]
            {
                "Table: Orders  (ExcludeFromMigrations)",
                "  Id            (PK)",
                "  Status",
                "  PlacedAt",
                "",
                "CustomerId:    NOT mapped — invisible to ShippingContext",
                "OrderLines:    NOT mapped — no navigation property exists",
                "Money columns: NOT mapped — no value object here",
                "",
                "ShippingContext cannot navigate to lines,",
                "cannot read CustomerId, cannot touch Money.",
                "The isolation is enforced by the EF model itself."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//


        // --- Screen 5: Live demo — same row, two different hydrated objects ---
        using var orderingScope = serviceProvider.CreateScope();
        var orderingCtx = orderingScope.ServiceProvider.GetRequiredService<OrderingContext>();

        using var shippingScope = serviceProvider.CreateScope();
        var shippingCtx = shippingScope.ServiceProvider.GetRequiredService<ShippingContext>();

        var order = await SeedDataHelper.EnsureOrderWithLinesAsync(orderingCtx);

        var shipment = await shippingCtx.Shipments
            .OrderBy(s => s.PlacedAt)
            .FirstAsync();

        var firstLine = order.Lines.First();

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Live Demo — Same Row Queried From Both Contexts"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "orderingCtx.Orders.Include(o => o.Lines).FirstAsync()",
            new[]
            {
                $"Id:          {order.Id}",
                $"CustomerId:  {orderingCtx.Entry(order).Property<int>("CustomerId").CurrentValue}",
                $"Status:      {order.Status}",
                $"PlacedAt:    {order.PlacedAt:u}",
                $"Lines:       {order.Lines.Count} line(s)",
                $"  Line 1:    Qty {firstLine.Quantity} x " +
                    $"{firstLine.UnitPrice.Amount:F2} {firstLine.UnitPrice.Currency}",
                $"Total:       {order.Total.Amount:F2} {order.Total.Currency}",
                "",
                "CustomerId, Lines collection, and Money are fully hydrated.",
                "The rich aggregate is ready for domain operations."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "shippingCtx.Shipments.FirstAsync()  — same primary key, same row",
            new[]
            {
                $"Id:       {shipment.Id}",
                $"Status:   {shipment.Status}",
                $"PlacedAt: {shipment.PlacedAt:u}",
                "",
                $"Ids match: {order.Id == shipment.Id}   <-- same database row",
                "",
                "CustomerId:  not mapped — not loaded — not visible here",
                "Lines:       not mapped — no navigation property on this type",
                "Money:       not mapped — no value object on ShipmentReadModel",
                "",
                "Same row. Completely different CLR objects.",
                "EF projected only what ShippingContext declared in its model."
            }
        ));

        
        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 6: Summary ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Summary — Why EF Core Allows Two Mappings of One Table"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Key takeaways",
            new[]
            {
                "1. EF Core builds a separate compiled model per DbContext.",
                "   OrderingContext.Model knows nothing about ShipmentReadModel.",
                "   ShippingContext.Model knows nothing about Order or OrderLine.",
                "",
                "2. ExcludeFromMigrations() prevents schema duplication.",
                "   ShippingContext never generates a migration for the Orders table.",
                "   OrderingContext owns the schema. ShippingContext only reads it.",
                "",
                "3. The CLR types are completely different objects.",
                "   Order enforces invariants, raises domain events, has private fields.",
                "   ShipmentReadModel is a plain class with only 3 public properties.",
                "",
                "4. This is bounded context isolation at the data layer.",
                "   Each context sees exactly the data its domain needs.",
                "   No accidental reads. No accidental writes. No coupling."
            }
        ));
    }
}
