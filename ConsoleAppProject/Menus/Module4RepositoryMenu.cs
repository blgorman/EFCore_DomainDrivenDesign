using ConsoleHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleAppProject.Menus;

public class Module4RepositoryMenu : BaseMenu
{
    private readonly IServiceProvider _serviceProvider;

    public Module4RepositoryMenu(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override List<string> MenuOptions() => new List<string>
    {
        "Generic Repo Anti-Pattern",
        "Only Aggregate Roots Get Repositories",
        "Repository Interface in Domain Layer",
        "OrderRepository Implementation",
        "Aggregate Loading (Include Strategy)",
        "Persisting Aggregates (Write Side)",
        "Unit of Work / Transaction Scope",
        "Testing Repositories with Testcontainers",
        "DEMO: Complete Order Repository",
        "Back"
    };

    public override async Task<bool> HandleMenuChoiceAsync(int choice)
    {
        using var scope = _serviceProvider.CreateScope();
        switch (choice)
        {
            case 1: await CodeAndDemonstrations.DemonstrateGenericRepoAntiPattern.ContrastApproachesAsync(scope.ServiceProvider); break;
            case 2: await CodeAndDemonstrations.DemonstrateOrderRepositoryInterface.WalkInterfaceAsync(); break;
            case 3: await CodeAndDemonstrations.DemonstrateRepositoryInterfaceInDomain.WalkDomainOwnershipAsync(scope.ServiceProvider); break;
            case 4: await CodeAndDemonstrations.DemonstrateOrderRepository.WalkImplementationAsync(scope.ServiceProvider); break;
            case 5: await CodeAndDemonstrations.DemonstrateAggregateLoading.ShowIncludeStrategyAsync(scope.ServiceProvider); break;
            case 6: await CodeAndDemonstrations.DemonstratePersistAggregate.ShowWriteSideAsync(scope.ServiceProvider); break;
            case 7: await CodeAndDemonstrations.DemonstrateUnitOfWork.ShowUoWBehaviorAsync(scope.ServiceProvider); break;
            case 8: await CodeAndDemonstrations.DemonstrateRepositoryTesting.ShowTestingApproachAsync(); break;
            case 9: await CodeAndDemonstrations.DemonstrateRepositoryEndToEnd.RunEndToEndAsync(scope.ServiceProvider); break;
            default: return false;
        }
        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //----------------------------------------------------------------//
        return true;
    }
}
