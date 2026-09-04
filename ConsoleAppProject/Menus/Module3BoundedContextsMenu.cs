using ConsoleHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleAppProject.Menus;

public class Module3BoundedContextsMenu : BaseMenu
{
    private readonly IServiceProvider _serviceProvider;

    public Module3BoundedContextsMenu(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override List<string> MenuOptions() => new List<string>
    {
        "Monolithic DbContext Anti-Pattern",
        "Focused DbContext (Ordering & Shipping)",
        "One Table, Two Models",
        "Cross-Context Reads",
        "DEMO: Multiple DbContexts",
        "Migration Strategy Terminal Demo",
        "Back"
    };

    public override async Task<bool> HandleMenuChoiceAsync(int choice)
    {
        using var scope = _serviceProvider.CreateScope();
        switch (choice)
        {
            case 1: await CodeAndDemonstrations.DemonstrateMonolithicContext.ShowProblemsAsync(); break;
            case 2: await CodeAndDemonstrations.DemonstrateFocusedDbContext.ShowOrderingContextAsync(scope.ServiceProvider); break;
            case 3: await CodeAndDemonstrations.DemonstrateOneTableTwoModels.ShowBothContextsAsync(scope.ServiceProvider); break;
            case 4: await CodeAndDemonstrations.DemonstrateCrossContextReads.ShowCrossContextProjectionAsync(scope.ServiceProvider); break;
            case 5: await CodeAndDemonstrations.DemonstrateMultipleContexts.RunEndToEndAsync(scope.ServiceProvider); break;
            case 6: await CodeAndDemonstrations.DemonstrateMigrationStrategy.ShowMigrationCommandsAsync(); break;
            default: return false;
        }
        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //----------------------------------------------------------------//
        return true;
    }
}
