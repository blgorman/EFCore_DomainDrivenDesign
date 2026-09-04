using ConsoleHelpers;

namespace ConsoleAppProject.Menus;

public class Module1IntroMenu : BaseMenu
{
    private readonly IServiceProvider _serviceProvider;

    public Module1IntroMenu(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override List<string> MenuOptions() => new List<string>
    {
        "Show Anemic Model",
        "Back"
    };

    public override async Task<bool> HandleMenuChoiceAsync(int choice)
    {
        switch (choice)
        {
            case 1:
                await CodeAndDemonstrations.DemonstrateAnemicModel.ShowAnemicOrderAsync();
                break;
            default:
                return false;
        }
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        return true;
    }
}
