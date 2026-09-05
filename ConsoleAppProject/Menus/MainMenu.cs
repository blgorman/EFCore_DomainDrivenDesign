using ConsoleHelpers;

namespace ConsoleAppProject.Menus;

public class MainMenu : BaseMenu
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Module1IntroMenu _module1Menu;
    private readonly Module2InvariantsMenu _module2Menu;
    private readonly Module3BoundedContextsMenu _module3Menu;
    private readonly Module4RepositoryMenu _module4Menu;
    private readonly Module5DomainEventsMenu _module5Menu;
    private readonly Module6SpecificationsMenu _module6Menu;

    public MainMenu(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _module1Menu = new Module1IntroMenu(_serviceProvider);
        _module2Menu = new Module2InvariantsMenu(_serviceProvider);
        _module3Menu = new Module3BoundedContextsMenu(_serviceProvider);
        _module4Menu = new Module4RepositoryMenu(_serviceProvider);
        _module5Menu = new Module5DomainEventsMenu(_serviceProvider);
        _module6Menu = new Module6SpecificationsMenu(_serviceProvider);
    }

    public override List<string> MenuOptions() => new List<string>
    {
        "Module 1: Introduction and DDD Concepts",
        "Module 2: Enforcing Domain Invariants",
        "Module 3: Bounded Contexts",
        "Module 4: Repository Pattern",
        "Module 5: Domain Events",
        "Module 6: Specification Pattern",
        "Module 7: CQRS and Outbox Next Steps",
        "Exit"
    };

    public override async Task<bool> HandleMenuChoiceAsync(int choice)
    {
        BaseMenu? next = null;
        string title = string.Empty;

        switch (choice)
        {
            case 1: next = _module1Menu; title = "Module 1: Introduction"; break;
            case 2: next = _module2Menu; title = "Module 2: Enforcing Invariants"; break;
            case 3: next = _module3Menu; title = "Module 3: Bounded Contexts"; break;
            case 4: next = _module4Menu; title = "Module 4: Repository Pattern"; break;
            case 5: next = _module5Menu; title = "Module 5: Domain Events"; break;
            case 6: next = _module6Menu; title = "Module 6: Specification Pattern"; break;
            case 7: next = new Module7NextStepsMenu(_serviceProvider); title = "Module 7: CQRS and Outbox Next Steps"; break;
            default: return false;
        }

        await next.ShowAsync(title);
        return true;
    }
}

