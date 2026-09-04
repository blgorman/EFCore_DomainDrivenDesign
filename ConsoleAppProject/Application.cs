using ConsoleAppProject.Menus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleAppProject;

public class Application
{
    private readonly MainMenu _menu;
    private readonly IConfiguration _configuration;
    public const int LINE_LENGTH = 40;
    private readonly ILogger<Application> _logger;

    public Application(IConfiguration configuration, ILogger<Application> logger, IServiceProvider serviceProvider)
    {
        _menu = new MainMenu(serviceProvider);
        _configuration = configuration;
        _logger = logger;
    }

    public async Task DoWork()
    {
        Console.WriteLine("Welcome to the EF Core with DDD Patterns Demo");

        await _menu.ShowAsync("Main Menu");

        Console.WriteLine("Thank you for using the EF Core with DDD Patterns Demo");
    }
}
