using System.ComponentModel;
using Innovia.Api.Common.Database;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Innovia.Api.Tests;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("innovia_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _container.GetConnectionString();
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        
        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }
}