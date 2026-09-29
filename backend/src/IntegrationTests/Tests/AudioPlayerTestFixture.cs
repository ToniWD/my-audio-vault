using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MusicStream.Core.Entities;

namespace MusicStream.IntegrationTests.Tests;

public class AudioPlayerTestFixture : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;

    public User User { get; private set; } = null!;
    public string AccessToken { get; private set; } = string.Empty;
    private readonly ILogger<AudioPlayerTestFixture> _logger;

    public AudioPlayerTestFixture(CustomWebApplicationFactory factory)
    {
        _factory = factory;


        using var scope = _factory.Services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<AudioPlayerTestFixture>();
    }

    public async ValueTask InitializeAsync()
    {
        var (testUser, _) = await UserUtils.CreateUserDB(_factory);
        User = testUser;

        _logger.LogInformation($"New user created with id: {testUser.Id}");

        var accessToken = await UserUtils.GetAccessTokenForExistingUser(testUser.Id.ToString(), _factory);

        AccessToken = accessToken;
    }

    public async ValueTask DisposeAsync()
    {
        if (User == null) return;

        await UserUtils.DeleteExistingUser(User, _factory);

        _logger.LogInformation($"Deleted the user with id: {User.Id}");
    }
}