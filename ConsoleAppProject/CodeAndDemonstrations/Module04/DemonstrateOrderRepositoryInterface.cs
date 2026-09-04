using ConsoleHelpers;
using OrderManagement.Domain.Aggregates;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;

namespace ConsoleAppProject.CodeAndDemonstrations;

// Module 4 Clip 2 — Only Aggregate Roots Get Repositories.
// Runs entirely in memory: no OrderingContext, no SaveChanges, no database.

public static class DemonstrateOrderRepositoryInterface
{
    public static async Task WalkInterfaceAsync()
    {
        // --- PART 1: The rule — proved by the type hierarchy ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 1: Only Aggregate Roots Get Repositories — Type Hierarchy at Runtime"), ConsoleColor.DarkBlue);

        var orderBase     = BaseTypeLabel(typeof(Order));
        var orderLineBase = BaseTypeLabel(typeof(OrderLine));
        var customerBase  = BaseTypeLabel(typeof(Customer));

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Domain class declarations — what the source actually says",
            new[]
            {
                "public class Order     : AggregateRoot  { ... }",
                "public class Customer  : AggregateRoot  { ... }",
                "public class OrderLine                  { ... }",
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 1b: Runtime Proof — BaseType for Each Class"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "BaseType for each domain class, read at runtime",
            new[]
            {
                $"typeof(Order).BaseType.Name     = \"{orderBase}\"",
                $"typeof(Customer).BaseType.Name  = \"{customerBase}\"",
                $"typeof(OrderLine).BaseType.Name = \"{orderLineBase}\"",
                "",
                "Rule: inherits AggregateRoot  =>  gets one repository interface in the domain",
                "Rule: plain class (no base)   =>  no repository, ever",
                "",
                $"Order     inherits AggregateRoot  =>  IOrderRepository exists      (yes)",
                $"Customer  inherits AggregateRoot  =>  ICustomerRepository exists   (yes)",
                $"OrderLine does not                =>  IOrderLineRepository exists  (no)",
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 2: Walk IOrderRepository method by method ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 2: IOrderRepository — The Interface in Full"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "IOrderRepository — actual source from OrderManagement.Domain/Repositories/IOrderRepository.cs",
            new[]
            {
                "public interface IOrderRepository",
                "{",
                "    Task<Order?>              GetByIdAsync(int id, CancellationToken ct = default);",
                "    void                       Add(Order order);",
                "    void                       Add(Order order, int customerId);",
                "    void                       Remove(Order order);",
                "    Task                       SaveAsync(CancellationToken ct = default);",
                "    Task<IReadOnlyList<Order>> ListAsync(ISpecification<Order> spec,",
                "                                        CancellationToken ct = default);",
                "}",
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 2b: Naive interface vs this interface ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 2b: What a Naive Repository Interface Looks Like"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "The naive version — what most people write first",
            new[]
            {
                "public interface IOrderRepository",
                "{",
                "    Task<Order?>              GetByIdAsync(int id);",
                "    Task<IEnumerable<Order>>  GetAllAsync();                    // unbounded SELECT *",
                "    Task<IQueryable<Order>>   QueryAsync();                     // leaks EF internals",
                "    Task                      UpdateAsync(Order order);         // redundant with EF tracking",
                "    Task                      AddAsync(Order order);",
                "    Task                      DeleteAsync(Order order);",
                "    Task<int>                 SaveChangesAsync(DbContext ctx);  // EF type in the domain!",
                "}",
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 2c: What Was Removed and Why"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Each removal is a deliberate design decision",
            new[]
            {
                "GetAllAsync   removed  =>  ListAsync(ISpecification<Order>) replaces it.",
                "                           Every query requires a filter. No unbounded SELECT *.",
                "",
                "UpdateAsync   removed  =>  EF change tracking detects all mutations automatically.",
                "                           Calling Update() marks every column dirty. Not needed.",
                "",
                "IQueryable    removed  =>  Query composition stays inside the Specification.",
                "                           Callers cannot tack on .Where() outside the repository.",
                "",
                "DbContext     removed  =>  No EF Core type crosses the domain boundary.",
                "                           The domain assembly compiles with zero reference to EF Core.",
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 3: Runtime scan — which repository interfaces exist in the domain? ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 3: Runtime Scan — Which Repository Interfaces Live in the Domain Assembly?"), ConsoleColor.DarkBlue);

        var domainAssembly = typeof(IOrderRepository).Assembly;
        var repoInterfaces = domainAssembly.GetTypes()
            .Where(t => t.IsInterface && t.Name.EndsWith("Repository"))
            .Select(t => $"  {t.Namespace}.{t.Name}")
            .ToArray();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            $"Scanning \"{domainAssembly.GetName().Name}\" for interfaces ending in 'Repository'",
            repoInterfaces
                .Prepend($"typeof(IOrderRepository).Assembly = \"{domainAssembly.GetName().Name}\"")
                .Prepend("-----------------------------------------")
                .Append("-----------------------------------------")
                .Append($"Total found: {repoInterfaces.Length}")
                .ToArray()
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 3b: What That Result Means"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Interpretation — two Aggregate Roots found, one child entity absent",
            new[]
            {
                "IOrderRepository      exists — Order is an Aggregate Root      (yes)",
                "ICustomerRepository   exists — Customer is an Aggregate Root   (yes)",
                "IOrderLineRepository  absent — OrderLine is a child entity      (no)",
                "",
                "Order and Customer each have their own identity and their own lifecycle,",
                "so each one gets exactly one repository. OrderLine has neither. It exists",
                "only inside an Order, which is why no repository was written for it.",
                "",
                "The absence of IOrderLineRepository is the enforcement mechanism.",
                "There is no interface to implement, no class to register in DI.",
                "A caller literally cannot get an IOrderLineRepository — it doesn't exist."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 4: Order guards — proved live ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 4: Order Guards — Place and Ship a Real Order"), ConsoleColor.DarkBlue);

        var customerId = Random.Shared.Next(1, int.MaxValue);
        var productId  = Random.Shared.Next(1, int.MaxValue);
        var price      = Money.Create(29.99m, "USD");
        var order      = Order.Place(customerId, new[] { (productId, 2, price) });

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Order.Place() — factory method, routes through the Aggregate Root",
            new[]
            {
                "var order = Order.Place(customerId, new[] { (productId, 2, price) });",
                "",
                $"  order.Id       = {order.Id}   <-- 0: nothing was saved, so SQL Server never assigned an identity",
                $"  order.Status   = {order.Status}",
                $"  order.Lines    = {order.Lines.Count} line(s)",
                "",
                "This demo runs entirely in memory. No DbContext, no SaveChanges, no database.",
                "The customer and product ids are random, because no rows are being written.",
                "",
                "order.Process();  order.Confirm();  order.Ship();",
            }
        ));

        order.Process();
        order.Confirm();
        order.Ship();

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "After Ship()",
            new[]
            {
                $"  order.Status = {order.Status}",
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 4b: Prove the guard fires ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 4b: The Guard — Try to Cancel a Shipped Order"), ConsoleColor.DarkBlue);

        string guardResult;
        try
        {
            order.Cancel();
            guardResult = "No exception — guard did NOT fire (this would be a bug)";
        }
        catch (InvalidOperationException ex)
        {
            guardResult = $"InvalidOperationException: {ex.Message}";
        }

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "order.Cancel() on a Shipped order — does the guard actually run?",
            new[]
            {
                "// From Order.cs:",
                "public void Cancel()",
                "{",
                "    if (Status == OrderStatus.Shipped)",
                "        throw new InvalidOperationException(\"Cannot cancel an order that has already been shipped.\");",
                "    ...",
                "}",
                "",
                $"Result: {guardResult}",
                "",
                "The business rule fired because Cancel() routes through Order.",
                "There is no other path. Order owns the lifecycle.",
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 4c: The bypass that doesn't exist ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 4c: The Bypass — What IOrderLineRepository Would Allow"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Hypothetical — this code CANNOT COMPILE because IOrderLineRepository does not exist",
            new[]
            {
                "// If IOrderLineRepository existed, a caller could write this:",
                "var lineRepo = sp.GetRequiredService<IOrderLineRepository>();",
                "var line = await lineRepo.GetByIdAsync(lineId);",
                "lineRepo.Remove(line);",
                "await lineRepo.SaveAsync();",
                "",
                "// What that silently bypasses:",
                "//   order.Cancel() guard  =>  Status == Shipped check never runs",
                "//   OrderCancelledEvent   =>  domain event never raised",
                "//   Any future invariant  =>  silently skipped forever",
                "",
                "// The fix is NOT to add guards on IOrderLineRepository methods.",
                "// The fix is: the interface does not exist.",
                "// Absence is the enforcement mechanism.",
            }
        ));

        await Task.CompletedTask;
    }

    private static string BaseTypeLabel(Type type)
    {
        var baseType = type.BaseType;
        return baseType is null || baseType == typeof(object) ? "(none)" : baseType.Name;
    }
}
