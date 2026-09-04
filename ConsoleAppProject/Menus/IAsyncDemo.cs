namespace ConsoleAppProject.Menus;

public interface IAsyncDemo
{
    List<string> MenuOptions();
    Task ShowAsync(string title);
    Task<bool> HandleMenuChoiceAsync(int choice);
}