using System.Runtime.InteropServices;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructure.Data;
using Testcontainers.MsSql;

namespace OrderManagement.Tests.Integration;

public class MsSqlFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = BuildContainer();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<OrderingContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using var ctx = new OrderingContext(options);
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    private static MsSqlContainer BuildContainer()
    {
        // The official Microsoft SQL Server image does not support ARM architecture, 
        // so we use Azure SQL Edge for ARM-based machines.  Azure SQL Edge does not support the sqlcmd tool
        // , so we use a different wait strategy to ensure the container is ready before running tests.
        var isArm = RuntimeInformation.ProcessArchitecture is Architecture.Arm or Architecture.Arm64;
        return isArm
            ? new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/azure-sql-edge")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
                .Build()
            : new MsSqlBuilder().Build();
    }
}



[CollectionDefinition("MsSql")]
public class MsSqlCollection : ICollectionFixture<MsSqlFixture> { }
