using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderManagement.Application.Handlers;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Events;
using OrderManagement.Infrastructure.Repositories;
using Serilog;

namespace ConsoleAppProject;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.ResetColor();

        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty;
        var logConsole = Environment.GetEnvironmentVariable("LOG_TO_CONSOLE") ?? "false";
        var logFile = Environment.GetEnvironmentVariable("LOG_TO_FILE") ?? "false";
        var logToConsole = logConsole.Equals("true", StringComparison.OrdinalIgnoreCase);
        var logToFile = logFile.Equals("true", StringComparison.OrdinalIgnoreCase);
        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        if (logToFile)
        {
            SetupLogging(env, logToConsole, timeStamp);
        }

        var host = Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                config.AddEnvironmentVariables();
                config.AddUserSecrets<Program>();
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("ApplicationDbConnection")
                    ?? throw new InvalidOperationException("Connection string 'ApplicationDbConnection' not found.");

                // Domain event infrastructure
                services.AddScoped<DomainEventDispatcher>();
                services.AddScoped<DomainEventDispatchInterceptor>();
                services.AddScoped<IDomainEventHandler<OrderPlacedEvent>, OrderPlacedEventHandler>();
                services.AddScoped<IDomainEventHandler<OrderShippedEvent>, OrderShippedEventHandler>();
                services.AddScoped<IDomainEventHandler<OrderCancelledEvent>, OrderCancelledEventHandler>();

                // Write-side context with interceptor
                services.AddDbContext<OrderingContext>((sp, options) =>
                    options.UseSqlServer(connectionString)
                           .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

                // Read-side context — separate migration history
                services.AddDbContext<ShippingContext>(options =>
                    options.UseSqlServer(connectionString,
                        o => o.MigrationsHistoryTable("__EFMigrationsHistory_Shipping")));

                // Repository and handlers
                services.AddScoped<IOrderRepository, OrderRepository>();
                services.AddScoped<ICustomerRepository, CustomerRepository>();
                services.AddScoped<PlaceOrderHandler>();

                services.AddTransient<Application>();
            }).Build();

        // Drop and re-apply migrations on every run — guarantees a clean schema for demos
        using (var scope = host.Services.CreateScope())
        {
            var ordering  = scope.ServiceProvider.GetRequiredService<OrderingContext>();
            var shipping  = scope.ServiceProvider.GetRequiredService<ShippingContext>();
            await ordering.Database.EnsureDeletedAsync(); //new database for every demo
            await ordering.Database.MigrateAsync();
            await shipping.Database.MigrateAsync();
        }

        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine($"Environment:     {env}");
        Console.WriteLine($"Console logging: {(logToConsole ? "ON" : "OFF")}");
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine("Press any key to continue");
        if (!Console.IsInputRedirected) Console.ReadKey();

        using var appScope = host.Services.CreateScope();
        var app = appScope.ServiceProvider.GetRequiredService<Application>();
        await app.DoWork();

        if (logToFile)
        {
            Log.CloseAndFlush();
        }
    }

    private static void SetupLogging(string env, bool logToConsole, string timeStamp)
    {
        var logPath = GetLogPath(timeStamp);
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day);

        if (logToConsole)
        {
            loggerConfig = loggerConfig.WriteTo.Console();
        }

        Log.Logger = loggerConfig.CreateLogger();
    }

    private static string GetLogPath(string timeStamp)
    {
        var directory = @"C:\Logs";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return $@"{directory}\logfile_{timeStamp}.txt";
    }
}
