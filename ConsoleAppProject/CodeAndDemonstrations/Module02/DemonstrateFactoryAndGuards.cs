using ConsoleHelpers;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.ValueObjects;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateFactoryAndGuards
{
    public static async Task ShowFactoryAndGuardsAsync()
    {
        // --- PART 1: The pattern — private constructor forces all creation through the factory ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Factory Methods and Guard Clauses — Part 1: The Pattern"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "The enforcing structure: private constructor + static factory",
            new[]
            {
                "Both Money and OrderLine use private constructors.",
                "Order uses a private constructor (private = EF only, not application code).",
                "",
                "This means there is exactly ONE path to a valid instance: the factory method.",
                "Guards live inside the factory, before the constructor is ever called.",
                "If any guard fails, the object is never created — invalid state is impossible.",
                "",
                "Commented-out lines below prove the bypass is closed at compile time:",
                "  // var m = new Money(10m, \"USD\");       CS0122 — inaccessible",
                "  // var l = new OrderLine(pid, 1, m);    CS0122 — inaccessible (internal)",
                "  // var o = new Order();                  CS0122 — inaccessible (private)"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 2: Money guards ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 2: Money.Create guard clauses"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Money.Create — guard code",
            new[]
            {
                "public static Money Create(decimal amount, string currency)",
                "{",
                "    if (amount < 0)",
                "        throw new ArgumentException(\"Amount cannot be negative.\", nameof(amount));",
                "    if (string.IsNullOrWhiteSpace(currency))",
                "        throw new ArgumentException(\"Currency cannot be empty.\", nameof(currency));",
                "    return new Money(amount, currency);   // private ctor — only reachable here",
                "}"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // Guard 1: negative amount
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 2: Money.Create — Guard 1: Negative Amount Rejected"), ConsoleColor.DarkBlue);
        try
        {
            var _ = Money.Create(-5m, "USD");
        }
        catch (ArgumentException ex)
        {
            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "Money.Create(-5m, \"USD\") — guard fired",
                new[]
                {
                    $"Exception:  {ex.GetType().Name}",
                    $"Message:    {ex.Message}",
                    "",
                    "Amount < 0 triggered the guard. No Money was constructed."
                }
            ));
        }

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // Guard 2: empty currency
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 2: Money.Create — Guard 2: Empty Currency Rejected"), ConsoleColor.DarkBlue);
        try
        {
            var _ = Money.Create(10m, "");
        }
        catch (ArgumentException ex)
        {
            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "Money.Create(10m, \"\") — guard fired",
                new[]
                {
                    $"Exception:  {ex.GetType().Name}",
                    $"Message:    {ex.Message}",
                    "",
                    "Empty currency triggered the guard. No Money was constructed."
                }
            ));
        }

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 3: OrderLine constructor guards (internal — called from Order.AddLine) ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 3: OrderLine constructor guard clauses"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderLine — guard code (internal constructor, called only from Order.AddLine)",
            new[]
            {
                "internal OrderLine(int productId, int quantity, Money unitPrice)",
                "{",
                "    if (quantity <= 0)",
                "        throw new ArgumentException(\"Quantity must be greater than zero.\", nameof(quantity));",
                "    ArgumentNullException.ThrowIfNull(unitPrice);",
                "    ...",
                "}",
                "",
                "internal means only code inside OrderManagement.Domain can call this.",
                "Application code cannot construct an OrderLine at all — only Order.AddLine() can."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // Guard: zero quantity via Order.Place -> AddLine
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 3: OrderLine — Guard: Zero Quantity Rejected"), ConsoleColor.DarkBlue);
        try
        {
            var _ = Order.Place(
                Random.Shared.Next(1, int.MaxValue),
                new[] { (SeedDataHelper.Product1Id, 0, Money.Create(10m, "USD")) }
            );
        }
        catch (ArgumentException ex)
        {
            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "Order.Place with quantity=0 — OrderLine guard fired",
                new[]
                {
                    $"Exception:  {ex.GetType().Name}",
                    $"Message:    {ex.Message}",
                    "",
                    "Quantity <= 0 rejected inside the OrderLine constructor.",
                    "The guard fires before the OrderLine is added to the Order."
                }
            ));
        }

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 4: Order.Place guards ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 4: Order.Place guard clauses"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Order.Place — guard code",
            new[]
            {
                "public static Order Place(int customerId,",
                "    IEnumerable<(int productId, int quantity, Money unitPrice)> lines)",
                "{",
                "    ArgumentNullException.ThrowIfNull(lines);",
                "    var linesList = lines.ToList();",
                "    if (linesList.Count == 0)",
                "        throw new ArgumentException(\"An order must have at least one line.\", nameof(lines));",
                "    ...",
                "}"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // Guard 1: null lines
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 4: Order.Place — Guard 1: Null Lines Rejected"), ConsoleColor.DarkBlue);
        try
        {
            var _ = Order.Place(Random.Shared.Next(1, int.MaxValue), null!);
        }
        catch (ArgumentNullException ex)
        {
            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "Order.Place(customerId, null) — guard fired",
                new[]
                {
                    $"Exception:  {ex.GetType().Name}",
                    $"Message:    {ex.Message}",
                    "",
                    "ArgumentNullException.ThrowIfNull(lines) caught the null before any work happened."
                }
            ));
        }

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // Guard 2: empty lines list
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 4: Order.Place — Guard 2: Empty Lines Rejected"), ConsoleColor.DarkBlue);
        try
        {
            var _ = Order.Place(Random.Shared.Next(1, int.MaxValue), Array.Empty<(int, int, Money)>());
        }
        catch (ArgumentException ex)
        {
            Console.Write(OutputHelpers.BoxedArrayWithTitle(
                "Order.Place(customerId, empty list) — guard fired",
                new[]
                {
                    $"Exception:  {ex.GetType().Name}",
                    $"Message:    {ex.Message}",
                    "",
                    "An order with zero lines violates the aggregate invariant.",
                    "The guard stops construction — a lineless Order can never exist."
                }
            ));
        }

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 5: Valid path — all guards pass, objects are constructed ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 5: Valid inputs — all guards pass"), ConsoleColor.DarkBlue);

        var price = Money.Create(29.99m, "USD");
        var order = Order.Place(
            Random.Shared.Next(1, int.MaxValue),
            new[]
            {
                (SeedDataHelper.Product1Id, 2, price),
                (SeedDataHelper.Product2Id, 1, Money.Create(9.99m, "USD"))
            }
        );

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Order.Place — succeeded with valid inputs",
            new[]
            {
                $"Id:           {order.Id}",
                $"Status:       {order.Status}",
                $"Lines:        {order.Lines.Count}",
                $"Line[0] Qty:  {order.Lines.First().Quantity}",
                $"Line[0] Price:{order.Lines.First().UnitPrice.Amount:C} {order.Lines.First().UnitPrice.Currency}",
                $"Line[1] Qty:  {order.Lines.Last().Quantity}",
                $"Line[1] Price:{order.Lines.Last().UnitPrice.Amount:C} {order.Lines.Last().UnitPrice.Currency}",
                $"Total:        {order.Total.Amount:C} {order.Total.Currency}",
                "Every object was constructed through its factory.",
                "Every guard passed. The aggregate is in a guaranteed-valid state."
            }
        ));

        await Task.CompletedTask;
    }
}
