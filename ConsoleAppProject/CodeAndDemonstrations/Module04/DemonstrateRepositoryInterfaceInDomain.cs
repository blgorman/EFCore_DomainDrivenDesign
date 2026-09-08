using ConsoleHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Domain.Repositories;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateRepositoryInterfaceInDomain
{
    public static async Task WalkDomainOwnershipAsync(IServiceProvider serviceProvider)
    {
        // --- PART 1: The Dependency Rule ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Repository Interface in Domain — Part 1: The Dependency Rule"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "The Dependency Rule: inner layers never depend on outer layers",
            new[]
            {
                "The layered architecture for this project:",
                "",
                "  Domain          =>  no dependencies on any other project layer",
                "  Application     =>  depends on Domain only",
                "  Infrastructure  =>  depends on Domain (implements its interfaces)",
                "  API / Console   =>  depends on all layers; wires up DI",
                "",
                "Where does IOrderRepository live?",
                "  OrderManagement.Domain.Repositories.IOrderRepository",
                "",
                "Where does the EF Core implementation live?",
                "  OrderManagement.Infrastructure.Repositories.OrderRepository",
                "",
                "Domain declares WHAT a repository must do.",
                "Infrastructure decides HOW it is done.",
                "Domain knows nothing about EF Core, SQL Server, or migrations."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 2: The interface contains zero EF Core types ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 2: IOrderRepository References No EF Core Types"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "IOrderRepository — every type each method touches",
            new[]
            {
                "Method                    Parameter/Return types",
                "-----------------------------------------------------",
                "GetByIdAsync(int, ct)    int             — System",
                "                          CancellationToken — System.Threading",
                "                          Order?            — OrderManagement.Domain",
                "",
                "Add(Order)                Order             — OrderManagement.Domain",
                "",
                "Add(Order, int)           Order             — OrderManagement.Domain",
                "                          int               — System",
                "",
                "Remove(Order)             Order             — OrderManagement.Domain",
                "",
                "SaveAsync(ct)             CancellationToken — System.Threading",
                "",
                "ListAsync(spec, ct)       ISpecification<Order> — Ardalis.Specification",
                "                          CancellationToken — System.Threading",
                "                          IReadOnlyList<Order> — System.Collections",
                "",
                "No DbContext. No DbSet. No IQueryable. No EntityEntry.",
                "Every type is either BCL, Domain, or the thin Ardalis.Specification abstraction."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 3: Runtime proof — assembly ownership ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 3: Infrastructure Depends on Domain — Domain Has No EF Core Dependency"), ConsoleColor.DarkBlue);

        var domainAssemblyName = typeof(IOrderRepository).Assembly.GetName().Name;

        var infrastructureAssemblyName = serviceProvider
            .GetRequiredService<IOrderRepository>()
            .GetType().Assembly.GetName().Name;

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Interface defined in Domain — implementation resolved from Infrastructure",
            new[]
            {
                $"typeof(IOrderRepository).Assembly.GetName().Name  =>  \"{domainAssemblyName}\"",
                $"GetRequiredService<IOrderRepository>().GetType().Assembly.GetName().Name  =>  \"{infrastructureAssemblyName}\""
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 3b: Does Domain Reference EF Core?"), ConsoleColor.DarkBlue);

        var domainAssembly = typeof(IOrderRepository).Assembly;
        var efCoreReference = domainAssembly.GetReferencedAssemblies()
            .FirstOrDefault(a => a.Name?.Contains("EntityFrameworkCore") == true);

        var methodParamTypes = typeof(IOrderRepository)
            .GetMethods()
            .SelectMany(m => m.GetParameters().Select(p => p.ParameterType))
            .Distinct()
            .Select(t => $"  {t.Assembly.GetName().Name,-40} {t.Name}")
            .ToArray();

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            $"Does \"{domainAssemblyName}\" reference EntityFrameworkCore?",
            new[]
            {
                $"Referenced EF Core assembly found: {(efCoreReference is not null ? efCoreReference.FullName : "none")}",
                "",
                efCoreReference is not null
                    ? "WARNING — Domain has an EF Core reference. This violates the Dependency Rule."
                    : "Confirmed: Domain has zero EF Core references.",
                "",
                "Parameter types used by IOrderRepository methods:",
            }.Concat(methodParamTypes).ToArray()
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- PART 4: What this unlocks ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Part 4: What the Dependency Rule Unlocks"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Three concrete benefits of keeping the interface in Domain",
            new[]
            {
                "1. Testability",
                "   Application handlers only import IOrderRepository.",
                "   Tests can mock or fake it — no database, no EF Core, no migrations.",
                "   A unit test file needs zero infrastructure setup to test business logic.",
                "",
                "2. Swappable persistence",
                "   Swap OrderRepository (EF Core + Postgres) for a Redis, Mongo, or",
                "   in-memory implementation without changing a single line in Domain",
                "   or Application. Only the Infrastructure project and DI wiring change.",
                "",
                "3. Independent compilation",
                "   Application handles compile and run without any EF Core package.",
                "   Dependency on Microsoft.EntityFrameworkCore stays inside Infrastructure,",
                "   so a NuGet update or breaking change in EF Core only touches that project."
            }
        ));

        await Task.CompletedTask;
    }
}
