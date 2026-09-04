using ConsoleHelpers;

namespace ConsoleAppProject.Menus;

public abstract class BaseMenu : IAsyncDemo
{
    public abstract Task<bool> HandleMenuChoiceAsync(int choice);

    public abstract List<string> MenuOptions();

    //not virtual so can't be overridden
    public async Task ShowAsync(string title)
    {
        bool shouldContinue = true;
        while (shouldContinue)
        {
            if (!Console.IsOutputRedirected) Console.Clear();

            var menuText = MenuGenerator.GenerateMenu(title, "Please select an operation", MenuOptions(), Application.LINE_LENGTH);

            // Show menu and get user choice
            int choice = InputHelpers.GetInputAsInt(menuText, confirm: true, min: 1, max: MenuOptions().Count);

            try
            {
                shouldContinue = await HandleMenuChoiceAsync(choice);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine("Press any key to continue...");
                if (!Console.IsInputRedirected) Console.ReadKey();
            }
        }
    }
}
