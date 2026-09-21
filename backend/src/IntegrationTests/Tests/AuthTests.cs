using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MusicStream.Application.DTOs;
using MusicStream.Core.Entities;

namespace MusicStream.IntegrationTests.Tests;

[Collection("IntegrationTestWithDBCollection")]
public class AuthTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ILogger<AuthTests> _logger;

    // User is static because all test from this class (collection) work in sequential order,
    // but for every test a new instance of this class is created 
    private static User _sharedUser;
    private static string _sharedUserPassword;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<AuthTests>();
    }

    public async ValueTask InitializeAsync()
    {
        if (_sharedUser is null)
        {
            (_sharedUser, _sharedUserPassword) = await UserUtils.CreateUserDB(_factory);
            _logger.LogInformation($"New user created for {this.GetType().Name} with id: {_sharedUser.Id}");
        }
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }


    // Helper DTO
    private record RegisterResponseDto(Guid UserId);

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Username: "johndoeregistervalid",
            FirstName: "John",
            LastName: "Doe",
            Email: $"john.doeregistervalid@example.com",
            Password: "Password123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.UserId);
    }

    [Theory]
    // Invalid username
    [InlineData("ab", "John", "Doe", "john@example.com", "Password123!")]
    [InlineData("user@name", "John", "Doe", "john@example.com", "Password123!")]

    // Invalid firstName / lastName
    [InlineData("valid_user", "", "Doe", "john@example.com", "Password123!")]
    [InlineData("valid_user", "John", "D", "john@example.com", "Password123!")]

    // Invalid email
    [InlineData("valid_user", "John", "Doe", "invalid-email.com", "Password123!")]
    [InlineData("valid_user", "John", "Doe", "", "Password123!")]

    // Invalid password
    [InlineData("valid_user", "John", "Doe", "john@example.com", "pass123!")]
    [InlineData("valid_user", "John", "Doe", "john@example.com", "SHORT1!")]
    public async Task Register_WithInvalidData_ReturnsBadRequest(
    string username, string firstName, string lastName, string email, string password)
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Username: username,
            FirstName: firstName,
            LastName: lastName,
            Email: email,
            Password: password
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            registerRequest,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequestOrConflict()
    {
        var requestWithDuplicateEmail = new RegisterRequest(
            Username: $"jane_{Guid.NewGuid():N}",
            FirstName: "Jane",
            LastName: "Doe",
            Email: _sharedUser.Email, // Already used email
            Password: "Password123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            requestWithDuplicateEmail,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Conflict,
            $"Expected BadRequest or Conflict, but received {response.StatusCode}");
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequestOrConflict()
    {
        var requestWithDuplicateUsername = new RegisterRequest(
            Username: _sharedUser.UserName, // Already used username
            FirstName: "Jane",
            LastName: "Doe",
            Email: $"new.email{Guid.NewGuid():N}@example.com",
            Password: "Password123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            requestWithDuplicateUsername,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Conflict,
            $"Expected BadRequest or Conflict, but received {response.StatusCode}");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        var loginRequest = new LoginRequest(
            Email: _sharedUser.Email!,
            Password: _sharedUserPassword
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(authResponse);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.RefreshToken));
        Assert.True(authResponse.AccessTokenExpirationTime > DateTime.UtcNow);
    }

    [Theory]
    [InlineData("nonexistent@example.com", "WrongPassword123!")] // Wrong email and password
    [InlineData("nonexistent@example.com", "Password123!")]      // Wrong email
    [InlineData("user.test.auth@example.com", "WrongPassword123!")]   // Wrong password
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized(string testEmail, string testPassword)
    {
        var loginRequest = new LoginRequest(testEmail, testPassword);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        string refreshToken = await UserUtils.GetRefreshTokenForExistingUser(_sharedUser.Id.ToString(), _factory);

        var refreshRequest = new RefreshTokenRequest(
            RefreshToken: refreshToken
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string newRefreshToken = await UserUtils.GetRefreshTokenForExistingUser(_sharedUser.Id.ToString(), _factory);

        var result = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(response);

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));

        Assert.NotEqual(refreshToken, result.RefreshToken);
        Assert.Equal(newRefreshToken, result.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        var refreshRequest = new RefreshTokenRequest(
            RefreshToken: "wrongToken"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}