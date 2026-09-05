using ConsoleHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateReadOnlyCollections
{
    public static async Task ShowCollectionAndShadowPropsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<OrderingContext>();
        var order = await SeedDataHelper.EnsureOrderWithLinesAsync(ctx);

        // --- PART 1: OrderLine already has a shadow FK — the reference example ---
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Part 1 — OrderLine shadow FK 'OrderId' (always present)"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderLine — Shadow FK 'OrderId' is already in place",
            new[]
            {
                "OrderLine has NO CLR 'OrderId' property anywhere in the class.",
                "EF manages the FK entirely as a shadow property.",
                "",
                "EF config (OrderConfiguration.cs):",
                "  builder.HasMany(o => o.Lines)",
                "      .WithOne()                  <-- no nav property back to Order",
                "      .HasForeignKey(\"OrderId\")",
                "      .IsRequired();",
                "",
                "OrderLine has NO navigation property back to Order.",
                "DDD boundary rule: OrderLine must not reference its aggregate root.",
                "EF manages the relationship via the shadow FK — no CLR integer needed."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        var shadowValues = order.Lines
            .Select(line =>
            {
                var shadowOrderId = ctx.Entry(line).Property<int>("OrderId").CurrentValue;
                return $"LineId: {line.Id} | Shadow FK 'OrderId': {shadowOrderId}";
            })
            .ToArray();

        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Reading the shadow FK via EF Entry API"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "ctx.Entry(line).Property<int>(\"OrderId\").CurrentValue",
            shadowValues.Concat(new[]
            {
                "",
                "The 'OrderId' column exists in the DB and is populated correctly.",
                "But there is no CLR property to read it from directly.",
                "The EF Entry API is the only way to inspect it at runtime."
            }).ToArray()
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 2: Apply the same concept to Order.CustomerId ---
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Part 2 — Order.CustomerId: same pattern, your turn"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Order.CustomerId — before Clip 5 changes",
            new[]
            {
                "Right now, Order.CustomerId is a public CLR property.",
                "Any caller can read order.CustomerId directly.",
                "",
                "Clip 5 student changes:",
                "  1. Remove: public int CustomerId { get; private set; }",
                "     Add nothing — no field, no property. EF owns CustomerId through shadow state.",
                "  2. In Order.Place(): remove CustomerId = customerId. Add nothing.",
                "  3. Update OrderConfiguration.cs:",
                "       builder.Property<int>(\"CustomerId\");",
                "       builder.HasOne<Customer>().WithMany()",
                "          .HasForeignKey(\"CustomerId\").IsRequired();",
                "  4. Before SaveChangesAsync in demo saves (see Clip 5 instructions):",
                "       ctx.Entry(order).Property<int>(\"CustomerId\").CurrentValue = CustomerAId;",
                "",
                "After this change, external callers cannot read CustomerId directly.",
                "EF owns it entirely as a shadow property — same as OrderLine.OrderId."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Part 3 — Reading Order.CustomerId via EF Entry API"),
            ConsoleColor.DarkBlue);

        var customerId = ctx.Entry(order).Property<int>("CustomerId").CurrentValue;

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "ctx.Entry(order).Property<int>(\"CustomerId\").CurrentValue",
            new[]
            {
                $"Order Id:    {order.Id}",
                $"CustomerId:  {customerId}   (shadow property — no CLR property on Order)",
                "",
                "Contrast:",
                "  Order.CustomerId   — shadow FK, no CLR property (as of Clip 5)",
                "",
                "Same EF Entry API. Same shadow property pattern. Both hide raw FK integers.",
                "The Customer entity is now usable via a navigation property if needed.",
                "No raw int FK leaks into the public surface of either aggregate entity."
            }
        ));
    }
}
