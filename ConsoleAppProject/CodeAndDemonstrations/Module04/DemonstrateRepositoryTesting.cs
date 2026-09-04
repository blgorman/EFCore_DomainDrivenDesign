using ConsoleHelpers;

namespace ConsoleAppProject.CodeAndDemonstrations;

// Module 4 Clip 8 — Testing Repositories with Testcontainers.
// This clip runs in OrderManagement.Tests via dotnet test, not in this console app.
// Runs entirely in memory: no OrderingContext, no SaveChanges, no database.

public static class DemonstrateRepositoryTesting
{
    public static async Task ShowTestingApproachAsync()
    {
        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Part 1: Why Not the In-Memory Provider or SQLite"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "This clip runs in the test project, not in this console app",
            new[]
            {
                "OrderRepository is verified by integration tests in OrderManagement.Tests.",
                "Run them from a terminal:",
                "",
                "    dotnet test --filter Module=4",
                "",
                "---",
                "UseInMemoryDatabase  ->  enforces no constraints, generates keys differently.",
                "SQLite in-process    ->  closer, but still not the production engine.",
                "Testcontainers.MsSql ->  a real SQL Server, the engine used in production.",
                "",
                "A broken Include chain, an owned entity that fails to load, or a primary key",
                "that never generates will all pass a test built on a mock."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Part 2: MsSqlFixture — One Container per Test Run"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderManagement.Tests/Integration/MsSqlFixture.cs",
            new[]
            {
                "BuildContainer()   starts Testcontainers.MsSql (Azure SQL Edge on ARM machines,",
                "                   because the official SQL Server image has no ARM build).",
                "InitializeAsync()  starts the container, then calls Database.MigrateAsync()",
                "                   against the container before any test runs.",
                "",
                "[CollectionDefinition(\"MsSql\")]",
                "public class MsSqlCollection : ICollectionFixture<MsSqlFixture> { }",
                "",
                "Every test class in the MsSql collection shares one container for the whole",
                "run, rather than starting one container per test.",
                "",
                "Docker must be running before dotnet test is invoked."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        OutputHelpers.WriteColored(
            OutputHelpers.SectionBanner("Part 3: The Two Module 4 Tests"),
            ConsoleColor.DarkBlue);

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderManagement.Tests/Integration/OrderRepositoryTests.cs",
            new[]
            {
                "Add_ThenGetById_ReturnsOrderWithLines",
                "    creates an Order, calls _repo.Add, then _repo.SaveAsync,",
                "    then reloads through _repo.GetByIdAsync and asserts the Lines came back.",
                "",
                "Remove_ThenGetById_ReturnsNull",
                "    saves an Order, removes it, saves again,",
                "    then asserts GetByIdAsync returns null.",
                "",
                "Assertions use Shouldly, so they read as .ShouldNotBeNull() and .ShouldBe().",
                "",
                "---",
                "Both tests fail against the stubbed OrderRepository and pass once Clip 4",
                "is implemented, with no change to the test file itself.",
                "",
                "ListAsync_WithOrdersByCustomerSpec_ReturnsOnlyThatCustomersOrders lives in the",
                "same file but is tagged Module 6. That test belongs to the Specification pattern."
            }
        ));

        Console.WriteLine();
        await Task.CompletedTask;
    }
}
