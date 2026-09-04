using ConsoleHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleAppProject.Menus;

public class Module5DomainEventsMenu : BaseMenu
{
    private readonly IServiceProvider _serviceProvider;

    public Module5DomainEventsMenu(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override List<string> MenuOptions() => new List<string>
    {
        "Domain Events Overview (IDomainEvent Walkthrough)",
        "Events on Aggregate Root",
        "SaveChanges Interceptor",
        "Event Dispatcher and Handler",
        "Pre-Save vs Post-Save Dispatch",
        "DEMO: OrderPlaced Complete Workflow",
        "Back"
    };

    public override async Task<bool> HandleMenuChoiceAsync(int choice)
    {
        using var scope = _serviceProvider.CreateScope();
        switch (choice)
        {
            case 1: await CodeAndDemonstrations.DemonstrateDomainEvents.WalkInterfacesAsync(); break;
            case 2: await CodeAndDemonstrations.DemonstrateDomainEventCollection.ShowEventCollectionAsync(scope.ServiceProvider); break;
            case 3: await CodeAndDemonstrations.DemonstrateSaveChangesInterceptor.ShowInterceptorAsync(scope.ServiceProvider); break;
            case 4: await CodeAndDemonstrations.DemonstrateEventDispatcher.ShowDispatcherAsync(scope.ServiceProvider); break;
            case 5: await CodeAndDemonstrations.DemonstratePrePostSaveDispatch.CompareDispatchTimingAsync(scope.ServiceProvider); break;
            case 6: await CodeAndDemonstrations.DemonstrateOrderPlacedEvent.RunEndToEndAsync(scope.ServiceProvider); break;
            default: return false;
        }
        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //----------------------------------------------------------------//
        return true;
    }
}
