using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateMonolithicContext
{
    public static async Task ShowProblemsAsync()
    {
        // --- PART 1: Same word, two different models ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("What Is a Bounded Context? — The Same Word, Two Different Models"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Ask the Ordering team: what is an Order?",
            new[]
            {
                "OrderId         — unique identifier for billing and customer service",
                "CustomerId      — who placed the order",
                "OrderLines[]    — line items with product, quantity, and unit price",
                "Total           — money value object (amount + currency)",
                "Status          — Pending, Confirmed, Cancelled",
                "PlacedAt        — timestamp the customer submitted the cart",
                "",
                "The Ordering context cares about WHAT was bought and by WHOM."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("What Is a Bounded Context? — The Same Word, Two Different Models"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Ask the Shipping team: what is an Order?",
            new[]
            {
                "OrderId         — links the shipment back to the originating order",
                "ShipToAddress   — street, city, postal code, country",
                "TrackingNumber  — carrier-assigned tracking identifier",
                "Carrier         — UPS, FedEx, USPS …",
                "ShipmentStatus  — ReadyToShip, Shipped, Delivered, Lost",
                "ShippedAt       — timestamp the parcel left the warehouse",
                "",
                "The Shipping context cares about WHERE it goes and HOW it gets there."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("What Is a Bounded Context? — The Same Word, Two Different Models"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "The takeaway: 'Order' has a different meaning in each context",
            new[]
            {
                "Both teams use the word 'Order' — but they are NOT the same thing.",
                "",
                "Ordering Order:  CustomerId, OrderLines, Total, PlacedAt, Status",
                "Shipping Order:  ShipToAddress, TrackingNumber, Carrier, ShippedAt",
                "",
                "A Bounded Context draws a linguistic boundary around a model.",
                "Inside that boundary the word 'Order' has one precise meaning.",
                "Outside that boundary, the same word may mean something entirely different.",
                "",
                "DDD calls the shared vocabulary inside one boundary the Ubiquitous Language."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 2: The monolithic DbContext anti-pattern ---
        // Build the EF model in memory — no DB connection is made until a query runs.
        var options = new DbContextOptionsBuilder<MonolithicContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MonolithicDemo;Integrated Security=true")
            .Options;

        await using var ctx = new MonolithicContext(options);

        var entityTypeNames = ctx.Model
            .GetEntityTypes()
            .Select(e => e.ClrType.Name)
            .OrderBy(n => n)
            .ToArray();

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("The Monolithic DbContext Anti-Pattern"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            $"MonolithicContext.Model.GetEntityTypes() — {entityTypeNames.Length} types registered",
            entityTypeNames
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Problem 1: God Class"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Problem 1: God Class",
            new[]
            {
                "A single DbContext with 13 entity types across multiple domains",
                "becomes a God class that knows about everything — orders,",
                "payments, shipping, catalogue, and more.",
                "",
                "Every team edits the same file, creating constant merge conflicts."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Problem 2: Mixed Bounded Context Concerns"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Problem 2: Mixed Bounded Context Concerns",
            new[]
            {
                "Orders, Shipments, Payments, Inventory, and Reviews each belong",
                "to a separate bounded context. Combining them in one DbContext",
                "couples unrelated domains at the persistence layer and forces",
                "every developer to understand the entire schema."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Problem 3: All Migrations in One Place"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Problem 3: All Migrations in One Place",
            new[]
            {
                "One DbContext means one migration history table.",
                "A schema change in Payments generates a migration that is",
                "tracked alongside Orders.",
                "",
                "Teams cannot deploy bounded contexts independently —",
                "every migration affects the whole application."
            }
        ));
    }

    // -------------------------------------------------------------------------
    // Stub entity types — represent what would exist across multiple bounded
    // contexts if they were all collapsed into a single DbContext. Each type
    // carries only a primary key so EF can build and return the model metadata
    // without any configuration or database connection.
    // -------------------------------------------------------------------------
    private sealed class Order     { public int Id { get; set; } }
    private sealed class OrderLine { public int Id { get; set; } }
    private sealed class Customer  { public int Id { get; set; } }
    private sealed class Product   { public int Id { get; set; } }
    private sealed class Inventory { public int Id { get; set; } }
    private sealed class Invoice   { public int Id { get; set; } }
    private sealed class Shipment  { public int Id { get; set; } }
    private sealed class Payment   { public int Id { get; set; } }
    private sealed class Address   { public int Id { get; set; } }
    private sealed class Review    { public int Id { get; set; } }
    private sealed class Category  { public int Id { get; set; } }
    private sealed class Supplier  { public int Id { get; set; } }
    private sealed class Discount  { public int Id { get; set; } }

    private sealed class MonolithicContext : DbContext
    {
        public MonolithicContext(DbContextOptions<MonolithicContext> options) : base(options) { }

        public DbSet<Order>     Orders     => Set<Order>();
        public DbSet<OrderLine> OrderLines => Set<OrderLine>();
        public DbSet<Customer>  Customers  => Set<Customer>();
        public DbSet<Product>   Products   => Set<Product>();
        public DbSet<Inventory> Inventory  => Set<Inventory>();
        public DbSet<Invoice>   Invoices   => Set<Invoice>();
        public DbSet<Shipment>  Shipments  => Set<Shipment>();
        public DbSet<Payment>   Payments   => Set<Payment>();
        public DbSet<Address>   Addresses  => Set<Address>();
        public DbSet<Review>    Reviews    => Set<Review>();
        public DbSet<Category>  Categories => Set<Category>();
        public DbSet<Supplier>  Suppliers  => Set<Supplier>();
        public DbSet<Discount>  Discounts  => Set<Discount>();
    }
}
