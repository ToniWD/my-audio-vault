using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MusicStream.Application.DTOs;
using MusicStream.Core.Entities;

namespace MusicStream.IntegrationTests.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ILogger<AuthTests> _logger;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<AuthTests>();
    }


    // Helper DTO
    private record RegisterResponseDto(Guid UserId);

    [Fact]
    public async Task Register_WithValidData_ReturnsOkAndUserId()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Username: "john_doe",
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            Password: "Password123!"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

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
    public async Task Register_WithInvalidValidationRules_ReturnsBadRequest(
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
        // Arrange
        var (existingUser, _) = await GetPreExistingTestUserAsync();

        var requestWithDuplicateEmail = new RegisterRequest(
            Username: "completely_new_username",
            FirstName: "Jane",
            LastName: "Doe",
            Email: existingUser.Email!, // Already used email
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
        // Arrange
        var (existingUser, _) = await GetPreExistingTestUserAsync();

        var requestWithDuplicateUsername = new RegisterRequest(
            Username: existingUser.UserName!, // Already used username
            FirstName: "Jane",
            LastName: "Doe",
            Email: "new.email.address@example.com",
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
    public async Task Login_WithValidCredentials_ReturnsOkAndTokens()
    {
        // Prerequire: Add a user
        var (user, password) = await GetPreExistingTestUserAsync();

        var loginRequest = new LoginRequest(
            Email: user.Email!,
            Password: password
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
        // Make sure the test user exists
        await GetPreExistingTestUserAsync();

        var loginRequest = new LoginRequest(testEmail, testPassword);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Gets a pre-existing test user or creates one.
    /// This user data should not be changed.
    /// </summary>
    private async Task<(User User, string Password)> GetPreExistingTestUserAsync()
    {
        const string testEmail = "user.test.auth@example.com";
        const string testPassword = "Password123!";
        const string testUsername = "userTestAuth";

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var existingUser = await userManager.FindByEmailAsync(testEmail);

        if (existingUser is not null)
        {
            return (existingUser, testPassword);
        }

        var newUser = new User
        {
            UserName = testUsername,
            Email = testEmail
        };

        var createResult = await userManager.CreateAsync(newUser, testPassword);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not create test user '{testEmail}': {errors}");
        }

        return (newUser, testPassword);
    }

}