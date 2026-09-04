using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateCrossContextReads
{
    public static async Task ShowCrossContextProjectionAsync(IServiceProvider serviceProvider)
    {
        // --- Screen 1: The friction point ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Cross-Context Reads: The Friction Point"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "The problem: Shipping needs data that Ordering owns",
            new[]
            {
                "Shipping wants to generate a dispatch notice:",
                "  \"Order #7 for Alice Andersen — Status: Shipped\"",
                "",
                "ShippingContext can see:     Id, Status, PlacedAt",
                "ShippingContext CANNOT see:  CustomerId, CustomerName, OrderLines",
                "",
                "CustomerName lives in the Customers table, owned by OrderingContext.",
                "ShippingContext has no mapping for Customer — by design.",
                "",
                "This is the most common friction point in bounded context design.",
                "There are three ways to solve it, each with different trade-offs.",
                "",
                "Option 1 — Projection via a thin query service",
                "Option 2 — Denormalized data duplicated at write time",
                "Option 3 — Cross-schema join with explicit coupling acknowledgment"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 2: Option 1 — Thin query service ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Option 1 — Read-Only Projection via Thin Query Service"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Pattern: Shipping calls a query service that wraps OrderingContext",
            new[]
            {
                "// Interface in Application layer — no EF reference, no Ordering types:",
                "public interface IOrderQueryService",
                "{",
                "    Task<string?> GetCustomerNameForOrderAsync(int orderId);",
                "}",
                "",
                "// Implementation in Infrastructure — wraps OrderingContext:",
                "public class OrderQueryService : IOrderQueryService",
                "{",
                "    private readonly OrderingContext _ctx;",
                "    public OrderQueryService(OrderingContext ctx) => _ctx = ctx;",
                "",
                "    public Task<string?> GetCustomerNameForOrderAsync(int orderId) =>",
                "        _ctx.Orders",
                "            .Where(o => o.Id == orderId)",
                "            .Join(_ctx.Customers, o => o.CustomerId, c => c.Id,",
                "                  (o, c) => c.Name)",
                "            .FirstOrDefaultAsync();",
                "}"
            }
        ));
        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Trade-offs for Option 1",
            new[]
            {
                "GOOD:",
                "  Shipping never imports Order, Customer, or OrderingContext.",
                "  The interface is domain-language: GetCustomerNameForOrder.",
                "  Runtime coupling only — no compile-time dependency on Ordering types.",
                "",
                "WATCH OUT:",
                "  One query per shipment if called in a loop — N+1 risk.",
                "  Batch with GetCustomerNamesForOrdersAsync(IEnumerable<int> ids).",
                "",
                "BEST FOR: application code where Shipping acts on per-order data."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 3: Option 2 — Denormalization at write time ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Option 2 — Denormalized Data Duplicated at Write Time"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Pattern: Shipping owns a copy of what it needs — written when the order is placed",
            new[]
            {
                "// Shipping owns a table with the data it needs:",
                "public class ShipmentSummary",
                "{",
                "    public int   OrderId      { get; set; }",
                "    public string CustomerName { get; set; } = \"\";  // duplicated",
                "    public OrderStatus Status  { get; set; }",
                "    public DateTime PlacedAt   { get; set; }",
                "}",
                "",
                "// An event handler fires when Order is placed — copies data to Shipping:",
                "public class OrderPlacedHandler : IEventHandler<OrderPlacedEvent>",
                "{",
                "    public async Task HandleAsync(OrderPlacedEvent e)",
                "    {",
                "        var name = await _queryService.GetCustomerNameForOrderAsync(e.OrderId);",
                "        _shippingCtx.ShipmentSummaries.Add(new ShipmentSummary",
                "        {",
                "            OrderId      = e.OrderId,",
                "            CustomerName = name ?? \"\",",
                "            Status       = OrderStatus.Placed,",
                "            PlacedAt     = e.PlacedAt",
                "        });",
                "        await _shippingCtx.SaveChangesAsync();",
                "    }",
                "}"
            }
        ));
        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Trade-offs for Option 2",
            new[]
            {
                "GOOD:",
                "  Shipping reads its own table — zero runtime coupling to Ordering.",
                "  Fast queries: the data is right there, no join needed at read time.",
                "  Ordering can evolve its Customer model without breaking Shipping reads.",
                "",
                "WATCH OUT:",
                "  CustomerName can go stale if the customer renames (rare but possible).",
                "  Write-side complexity: event handler must be reliable and idempotent.",
                "  Two writes per order: Orders table AND ShipmentSummaries table.",
                "",
                "BEST FOR: frequently queried data that changes rarely after write time."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 4: Option 3 — Cross-schema join ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Option 3 — Cross-Schema Join with Explicit Coupling"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Pattern: raw SQL join across the boundary — intentional and documented",
            new[]
            {
                "-- SQL: joins across what are logically two bounded contexts",
                "SELECT  o.Id, o.[Status], o.PlacedAt,",
                "        c.Name AS CustomerName",
                "FROM    Orders    AS o",
                "JOIN    Customers AS c ON c.Id = o.CustomerId",
                "WHERE   o.[Status] = 'Placed'",
                "",
                "// EF equivalent (in a dedicated reporting/read context):",
                "var results = await orderingCtx.Orders",
                "    .Join(orderingCtx.Customers,",
                "          o => o.CustomerId,",
                "          c => c.Id,",
                "          (o, c) => new { o.Id, o.Status, o.PlacedAt, CustomerName = c.Name })",
                "    .ToListAsync();"
            }
        ));
        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Trade-offs for Option 3",
            new[]
            {
                "GOOD:",
                "  Simple and fast — single SQL query, no intermediate type or handler.",
                "  No event handler, no extra table, no service interface to maintain.",
                "",
                "WATCH OUT:",
                "  Shipping code references Ordering's Customer and Order types directly.",
                "  Renaming columns or changing the Customer schema breaks this join.",
                "  The coupling is real — you are crossing a bounded context boundary.",
                "",
                "RULE OF THUMB:",
                "  Reports and admin dashboards: couple explicitly, own it, document it.",
                "  Application logic (e.g., dispatch queue): use Option 1 instead.",
                "  Never use Option 3 in domain or application layer code."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 5: Live demo — Option 1 projection against real data ---
        using var orderingScope = serviceProvider.CreateScope();
        var orderingCtx = orderingScope.ServiceProvider.GetRequiredService<OrderingContext>();

        await SeedDataHelper.SeedAsync(orderingCtx);

        // Simulate what the thin query service returns to Shipping —
        // a plain DTO with only the fields Shipping requested.
        var shippingProjection = await orderingCtx.Orders
            .Join(orderingCtx.Customers,
                  o => EF.Property<int>(o, "CustomerId"),
                  c => c.Id,
                  (o, c) => new
                  {
                      o.Id,
                      CustomerName = c.Name,
                      o.Status,
                      o.PlacedAt
                  })
            .OrderBy(x => x.PlacedAt)
            .Take(4)
            .ToListAsync();

        var projectionLines = shippingProjection
            .Select(x =>
                $"  {x.Id}  |  {x.CustomerName,-18}  |  {x.Status,-9}  |  {x.PlacedAt:d}")
            .ToArray();

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Live Demo — Option 1: Query Service Projection Result"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "orderingCtx.Orders.Join(Customers, ...) — what the query service returns to Shipping",
            projectionLines
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "What this means for the architecture",
            new[]
            {
                "Shipping received plain objects — no Order type, no Customer type.",
                "It called: GetCustomerNamesForOrdersAsync(orderIds) on the query service.",
                "The service executed against OrderingContext and returned simple DTOs.",
                "",
                "Shipping can now render:  \"Order for Alice Andersen — Status: Placed\"",
                "Without ever importing Order.cs, Customer.cs, or OrderingContext.cs.",
                "",
                "The query service is the seam. Shipping speaks application language.",
                "Ordering's persistence details never cross that boundary."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 6: Summary ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Summary — Choosing the Right Cross-Context Read Pattern"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Decision guide: one context needs data owned by another",
            new[]
            {
                "Option 1 — Thin query service",
                "  When:  app logic needs current data from another context per request.",
                "  Cost:  one interface + one implementation class.",
                "  Risk:  N+1 if called in a loop — batch the IDs.",
                "",
                "Option 2 — Denormalization at write time",
                "  When:  data is read frequently and changes rarely after creation.",
                "  Cost:  event handler + extra columns or table + staleness management.",
                "  Risk:  stale data if source changes; handler must be idempotent.",
                "",
                "Option 3 — Cross-schema join (explicit coupling)",
                "  When:  reporting, admin dashboards, one-off analytics queries.",
                "  Cost:  compile-time coupling between contexts.",
                "  Risk:  fragile under schema change; never in domain/app layer code.",
                "",
                "Default: start with Option 1.",
                "Upgrade to Option 2 when profiling shows the query service is a bottleneck.",
                "Allow Option 3 only in isolated reporting code with coupling documented."
            }
        ));
    }
}
