using ConsoleHelpers;

namespace ConsoleAppProject.CodeAndDemonstrations;

public static class DemonstrateMigrationStrategy
{
    public static async Task ShowMigrationCommandsAsync()
    {
        // --- Screen 1: The problem — why you need a strategy at all ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Migration Strategy for Multiple DbContexts"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "The problem: EF Core cannot guess which context to target",
            new[]
            {
                "With a single DbContext, the EF CLI needs no guidance:",
                "  dotnet ef migrations add Initial   <- works fine",
                "",
                "With multiple DbContext classes in the same assembly, this fails:",
                "  More than one DbContext was found. Specify which one to use.",
                "  Use the --context option to specify the name of the DbContext.",
                "",
                "We also need migration FILES to be organized per context.",
                "Without --output-dir, every context writes into the same Migrations/ folder,",
                "generating file-name collisions and making histories impossible to read.",
                "",
                "Strategy: one --context flag + one --output-dir per bounded context,",
                "and one __EFMigrationsHistory table per context in the database."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 2: The commands ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("The Commands — Adding and Applying Migrations Per Context"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "dotnet ef migrations add  — target each context explicitly",
            new[]
            {
                "# OrderingContext — owns Orders, OrderLines, Customers",
                "dotnet ef migrations add Initial `",
                "    --context OrderingContext `",
                "    --output-dir Migrations/Ordering `",
                "    --project OrderManagement.Infrastructure `",
                "    --startup-project ConsoleAppProject",
                "",
                "# ShippingContext — owns only its own tables (not the Orders table)",
                "dotnet ef migrations add Initial `",
                "    --context ShippingContext `",
                "    --output-dir Migrations/Shipping `",
                "    --project OrderManagement.Infrastructure `",
                "    --startup-project ConsoleAppProject"
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "dotnet ef database update  — apply each context independently",
            new[]
            {
                "dotnet ef database update `",
                "    --context OrderingContext `",
                "    --project OrderManagement.Infrastructure `",
                "    --startup-project ConsoleAppProject",
                "",
                "dotnet ef database update `",
                "    --context ShippingContext `",
                "    --project OrderManagement.Infrastructure `",
                "    --startup-project ConsoleAppProject",
                "",
                "Each update command reads only its own __EFMigrationsHistory table.",
                "They can be run in any order, on separate schedules, by different teams."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 3: What the resulting folder structure and history tables look like ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Result — Folder Structure and History Tables in This Project"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "OrderManagement.Infrastructure/Migrations/  (as it exists in this solution)",
            new[]
            {
                "Migrations/",
                "├── Ordering/",
                "│   ├── 20260402152300_Initial.cs              <- UP: creates Orders, OrderLines, Customers",
                "│   ├── 20260402152300_Initial.Designer.cs     <- EF snapshot metadata",
                "│   └── OrderingContextModelSnapshot.cs        <- current model state for diff",
                "│",
                "└── Shipping/",
                "    ├── 20260402152321_Initial.cs              <- UP: creates ShippingContext-owned tables",
                "    ├── 20260402152321_Initial.Designer.cs",
                "    └── ShippingContextModelSnapshot.cs",
                "",
                "No migration in Shipping/ touches the Orders table.",
                "ShipmentReadModelConfiguration uses ExcludeFromMigrations() for that table.",
                "Ordering owns Orders. Shipping just reads it."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Default behavior: both contexts write to the SAME history table",
            new[]
            {
                "Without any extra configuration, every DbContext in an assembly uses:",
                "  __EFMigrationsHistory",
                "",
                "That means OrderingContext and ShippingContext both record their migrations",
                "in the same table — which is fine as long as migration IDs never collide.",
                "",
                "But it makes the history harder to read: 'Initial' from Ordering and",
                "'Initial' from Shipping sit in the same table with different timestamps.",
                "More importantly, if you ever want to apply them independently or deploy",
                "to separate databases, a shared history table gets in the way.",
                "",
                "This project opts in to separate history tables — see Program.cs."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "This project: separate history table configured for ShippingContext",
            new[]
            {
                "// Program.cs — ShippingContext registration:",
                "services.AddDbContext<ShippingContext>(options =>",
                "    options.UseSqlServer(connectionString,",
                "        o => o.MigrationsHistoryTable(\"__EFMigrationsHistory_Shipping\")));",
                "",
                "Table name                        Context",
                "───────────────────────────────────────────────────────────",
                "__EFMigrationsHistory             OrderingContext  (default)",
                "__EFMigrationsHistory_Shipping    ShippingContext  (explicit)",
                "",
                "Note: the Shipping/Initial migration is intentionally EMPTY.",
                "ShipmentReadModel uses ExcludeFromMigrations() — ShippingContext maps",
                "the Orders table as a read-only projection but never creates or alters it.",
                "That table belongs to OrderingContext. Shipping just reads it."
            }
        ));

        //----------------------------------------------------------------//
        Console.WriteLine();
        InputHelpers.WaitForUserInput(ConsoleColor.DarkYellow);
        Console.WriteLine();
        //----------------------------------------------------------------//

        // --- Screen 4: Summary + rules of thumb ---
        OutputHelpers.WriteColored(OutputHelpers.SectionBanner("Summary — Migration Rules for Multiple Contexts"), ConsoleColor.DarkBlue);
        Console.Write(OutputHelpers.BoxedArrayWithTitle(
            "Four rules that keep multi-context migration histories clean",
            new[]
            {
                "1. Always use --context",
                "   Every migrations add / database update command must name its context.",
                "   Never let EF guess — it cannot when more than one context is present.",
                "",
                "2. Always use --output-dir",
                "   One folder per context under Migrations/.",
                "   Mixing migration files leads to timestamp collisions and unreadable diffs.",
                "",
                "3. Consider a separate MigrationsHistoryTable per context",
                "   By DEFAULT, all contexts share __EFMigrationsHistory.",
                "   That works, but sharing makes history harder to read and makes",
                "   independent deployment to separate databases harder later.",
                "   Opt in via MigrationsHistoryTable() if teams deploy independently.",
                "",
                "4. Use ExcludeFromMigrations for tables owned by another context",
                "   ShippingContext maps the Orders table as a read projection.",
                "   ExcludeFromMigrations() stops ShippingContext from generating DROP/CREATE",
                "   statements for a table it does not own.",
                "",
                "Follow these four rules and each bounded context evolves its schema",
                "independently, at its own cadence, with no interference."
            }
        ));

        await Task.CompletedTask;
    }
}
