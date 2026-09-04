using ConsoleHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleAppProject.Menus;

public class Module7NextStepsMenu : BaseMenu
{
    private readonly IServiceProvider _serviceProvider;

    public Module7NextStepsMenu(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override List<string> MenuOptions() => new List<string>
    {
        "DEMO: Transactional Outbox",
        "DEMO: CQRS Read Model Beside the Write Model",
        "Back"
    };

    public override async Task<bool> HandleMenuChoiceAsync(int choice)
    {
        using var scope = _serviceProvider.CreateScope();
        switch (choice)
        {
            case 1: await CodeAndDemonstrations.DemonstrateOutboxPattern.ShowOutboxAsync(scope.ServiceProvider); break;
            case 2: await CodeAndDemonstrations.DemonstrateCqrsReadModel.ShowReadModelAsync(scope.ServiceProvider); break;
            default: return false;
        }
        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //----------------------------------------------------------------//
        return true;
    }
}
