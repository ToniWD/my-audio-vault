using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using MusicStream.Infrastructure.Data;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using MusicStream.Application.Configuration;
using Microsoft.AspNetCore.TestHost;

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

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        using var dbContext = new AppDbContext(options);
        await dbContext.Database.MigrateAsync();
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

            var testAuthSettings = new AuthenticationSettings
            {
                TokenSecret = $"MUSIC_STREAM_TEST_TOKEN_SECRET_KEY_32_BYTES_LONG",
                RefreshTokenSecret = $"MUSIC_STREAM_TEST_REFRESH_TOKEN_SECRET_KEY_32_BYTES_LONG",
                Issuer = "MusicStreamTestIssuer",
                Audience = "MusicStreamTestAudience"
            };

            services.RemoveAll(typeof(IOptions<AuthenticationSettings>));
            services.AddSingleton<IOptions<AuthenticationSettings>>(Options.Create(testAuthSettings));
        });

        base.ConfigureWebHost(builder);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}