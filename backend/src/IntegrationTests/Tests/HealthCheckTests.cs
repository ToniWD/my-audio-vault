namespace MusicStream.IntegrationTests.Tests;

public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public HealthCheckTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Check if the web api can be accessed after build
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetHealth_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/api/auth/health");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }
}