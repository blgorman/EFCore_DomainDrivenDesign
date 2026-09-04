using ConsoleHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleAppProject.Menus;

public class Module6SpecificationsMenu : BaseMenu
{
    private readonly IServiceProvider _serviceProvider;

    public Module6SpecificationsMenu(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override List<string> MenuOptions() => new List<string>
    {
        "Query Proliferation Problem",
        "Base Specification Class",
        "Spec with Include and OrderBy",
        "SpecificationEvaluator in Repository",
        "Dynamic Specification Composition",
        "DEMO: Dynamic Order Search End to End",
        "Back"
    };

    public override async Task<bool> HandleMenuChoiceAsync(int choice)
    {
        using var scope = _serviceProvider.CreateScope();
        switch (choice)
        {
            case 1: await CodeAndDemonstrations.DemonstrateQueryProliferation.ShowProliferationAsync(scope.ServiceProvider); break;
            case 2: await CodeAndDemonstrations.DemonstrateSpecificationBase.ShowBaseSpecAsync(scope.ServiceProvider); break;
            case 3: await CodeAndDemonstrations.DemonstrateSpecificationWithIncludes.ShowIncludesAsync(scope.ServiceProvider); break;
            case 4: await CodeAndDemonstrations.DemonstrateSpecificationEvaluator.ShowEvaluatorAsync(scope.ServiceProvider); break;
            case 5: await CodeAndDemonstrations.DemonstrateDynamicSpecComposition.ShowCompositionAsync(scope.ServiceProvider); break;
            case 6: await CodeAndDemonstrations.DemonstrateOrderSearch.RunEndToEndAsync(scope.ServiceProvider); break;
            default: return false;
        }
        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        //----------------------------------------------------------------//
        return true;
    }
}
