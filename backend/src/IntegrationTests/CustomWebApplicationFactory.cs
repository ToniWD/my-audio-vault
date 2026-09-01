using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using MusicStream.Infrastructure.Data;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MusicStream.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18")
    .WithDatabase("musicStream_test_db")
    .WithUsername("postgres_tests")
    .WithPassword("integration_tests_db")
    .Build();

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove original configuration for AppDbContext from program
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            // Add a new AppDbContext
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            // Run EF core migrations for test db
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        });
        base.ConfigureWebHost(builder);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}