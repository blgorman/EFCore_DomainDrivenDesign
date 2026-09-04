using ConsoleAppProject.CodeAndDemonstrations;
using ConsoleHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Infrastructure.Data;

namespace ConsoleAppProject.Menus;

public class Module2InvariantsMenu : BaseMenu
{
    private readonly IServiceProvider _serviceProvider;

    public Module2InvariantsMenu(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override List<string> MenuOptions() => new List<string>
    {
        "Anemic vs Domain Model Contrast",
        "Backing Field Mapping",
        "Private Setters vs Private Fields",
        "Owned Entities (Money Value Object)",
        "Shadow Properties — Hiding the FK",
        "Fluent IEntityTypeConfiguration Walkthrough",
        "EF Core Materialization",
        "Factory Methods and Guard Clauses",
        "DEMO: Order Aggregate End to End",
        "Back"
    };

    public override async Task<bool> HandleMenuChoiceAsync(int choice)
    {
        using (var seedScope = _serviceProvider.CreateScope())
        {
            var seedCtx = seedScope.ServiceProvider.GetRequiredService<OrderingContext>();
            await SeedDataHelper.SeedAsync(seedCtx);
        }

        using var scope = _serviceProvider.CreateScope();
        switch (choice)
        {
            case 1: await CodeAndDemonstrations.DemonstrateAnemicVsDomainModel.ContrastModelsAsync(); break;
            case 2: await CodeAndDemonstrations.DemonstrateBackingFields.ShowBackingFieldConfigAsync(scope.ServiceProvider); break;
            case 3: await CodeAndDemonstrations.DemonstratePrivateSettersVsFields.CompareApproachesAsync(scope.ServiceProvider); break;
            case 4: await CodeAndDemonstrations.DemonstrateOwnedEntities.ShowMoneyMappingAsync(scope.ServiceProvider); break;
            case 5: await CodeAndDemonstrations.DemonstrateReadOnlyCollections.ShowCollectionAndShadowPropsAsync(scope.ServiceProvider); break;
            case 6: await CodeAndDemonstrations.DemonstrateFluentConfiguration.HighlightFluentConfigurationAsync(scope.ServiceProvider); break;
            case 7: await CodeAndDemonstrations.DemonstrateEFMaterialization.ShowMaterializationWithProof(scope.ServiceProvider); break;
            case 8: await CodeAndDemonstrations.DemonstrateFactoryAndGuards.ShowFactoryAndGuardsAsync(); break;
            case 9: await CodeAndDemonstrations.DemonstrateOrderAggregate.RunEndToEndAsync(scope.ServiceProvider); break;
            default: return false;
        }
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        return true;
    }
}
