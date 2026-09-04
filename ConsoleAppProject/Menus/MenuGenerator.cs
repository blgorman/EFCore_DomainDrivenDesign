using System.Text;

namespace ConsoleAppProject.Menus;

public class MenuGenerator
{
    public static string GenerateMenu(string menuHeader, string menuPrompt, string[] menuLines, int lineLength)
    {
        var sb = new StringBuilder();
        //Header
        sb.AppendLine(new string('*', lineLength));
        sb.AppendLine(menuHeader);
        sb.AppendLine(new string('*', lineLength));
        //prompt
        sb.AppendLine($"* {menuPrompt}");
        sb.AppendLine(new string('-', lineLength));
        //menu
        int i = 1;
        foreach (var line in menuLines)
        {
            sb.AppendLine($"{i++}] {line}");
        }
        sb.AppendLine(new string('*', Application.LINE_LENGTH));
        return sb.ToString();
    }

    public static string GenerateMenu(string menuHeader
                                            , string menuPrompt
                                            , List<string> menuOptions
                                            , int lineLength)
    {
        return GenerateMenu(menuHeader, menuPrompt, menuOptions.ToArray(), lineLength);
    }
}
