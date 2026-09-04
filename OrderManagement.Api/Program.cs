using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Commands;
using OrderManagement.Application.Handlers;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Events;
using OrderManagement.Infrastructure.Repositories;

namespace OrderManagement.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddScoped<DomainEventDispatcher>();
        builder.Services.AddScoped<DomainEventDispatchInterceptor>();
        builder.Services.AddScoped<IDomainEventHandler<OrderPlacedEvent>, OrderPlacedEventHandler>();
        builder.Services.AddScoped<IDomainEventHandler<OrderShippedEvent>, OrderShippedEventHandler>();
        builder.Services.AddScoped<IDomainEventHandler<OrderCancelledEvent>, OrderCancelledEventHandler>();

        builder.Services.AddDbContext<OrderingContext>((sp, options) =>
            options.UseSqlServer(connectionString)
                   .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        builder.Services.AddDbContext<ShippingContext>(options =>
            options.UseSqlServer(connectionString,
                o => o.MigrationsHistoryTable("__EFMigrationsHistory_Shipping")));

        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<PlaceOrderHandler>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
