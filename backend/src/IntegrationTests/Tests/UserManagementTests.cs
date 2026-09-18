using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MusicStream.Application.DTOs;
using MusicStream.Core.Entities;

namespace MusicStream.IntegrationTests.Tests;

[Collection("IntegrationTestWithDBCollection")]
public class UserManagementTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ILogger<UserManagementTests> _logger;

    // User is static because all test from this class (collection) work in sequential order,
    // but for every test a new instance of this class is created 
    private static User _sharedUser;
    private static string _sharedUserPassword;

    public UserManagementTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<UserManagementTests>();
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

    [Theory]
    [InlineData("alex12", "Alex", "Jobs", "alex12test@example.com")]
    [InlineData("onlyUserName", null, null, null)]
    [InlineData(null, "OnlyFirstName", null, null)]
    [InlineData(null, null, "OnlyLastName", null)]
    [InlineData(null, null, null, "onlyemail@example.com")]
    [InlineData("Username", null, "LastName", null)]
    public async Task PatchProfile_WithValidData_ReturnNoContent(
    string? username, string? firstName, string? lastName, string? email)
    {
        var (testUser, _) = await UserUtils.CreateUserDB(_factory);

        var accessToken = await UserUtils.GetAccessTokenForExistingUser(testUser.Id.ToString(), _factory);

        var request = new PatchUserRequest(
            Username: username,
            FirstName: firstName,
            LastName: lastName,
            Email: email
        );

        var httpRequest = new HttpRequestMessage(HttpMethod.Patch, "/api/users/me")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert StatusCode
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updatedUser = await UserUtils.GetExistingUser(testUser.Id.ToString(), _factory);

        Assert.Equal(username ?? testUser.UserName, updatedUser.UserName);
        Assert.Equal(firstName ?? testUser.FirstName, updatedUser.FirstName);
        Assert.Equal(lastName ?? testUser.LastName, updatedUser.LastName);
        Assert.Equal(email ?? testUser.Email, updatedUser.Email);

        await UserUtils.DeleteExistingUser(updatedUser, _factory);
    }

    [Theory]
    // Invalid username
    [InlineData("ab", "John", "Doe", "john@example.com")]
    [InlineData("user@name", "John", "Doe", "john@example.com")]

    // Invalid firstName / lastName
    [InlineData(null, "", "Doe", "john@example.com")]
    [InlineData(null, "John", "D", "john@example.com")]

    // Invalid email
    [InlineData("valid_user", null, "Doe", "invalid-email.com")]
    [InlineData(null, null, "Doe", "")]
    // No payload
    [InlineData(null, null, null, null)]
    public async Task PatchProfile_WithInvalidData_ReturnBadRequest(
        string? username, string? firstName, string? lastName, string? email
    )
    {
        var (testUser, _) = await UserUtils.CreateUserDB(_factory);

        var accessToken = await UserUtils.GetAccessTokenForExistingUser(testUser.Id.ToString(), _factory);

        var request = new PatchUserRequest(
            Username: username,
            FirstName: firstName,
            LastName: lastName,
            Email: email
        );

        var httpRequest = new HttpRequestMessage(HttpMethod.Patch, "/api/users/me")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert StatusCode
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var updatedUser = await UserUtils.GetExistingUser(testUser.Id.ToString(), _factory);

        Assert.Equal(testUser.UserName, updatedUser.UserName);
        Assert.Equal(testUser.FirstName, updatedUser.FirstName);
        Assert.Equal(testUser.LastName, updatedUser.LastName);
        Assert.Equal(testUser.Email, updatedUser.Email);

        await UserUtils.DeleteExistingUser(updatedUser, _factory);
    }

    [Fact]
    public async Task PatchProfile_WithInvalidAccessToken_ReturnBadRequest()
    {
        var accessToken = await UserUtils.GetAccessTokenForExistingUser(_sharedUser.Id.ToString(), _factory);
        StringBuilder sb = new StringBuilder(accessToken);

        if (sb[32] != 'c') sb[32] += 'c';
        else sb[32] = 'a';

        var request = new PatchUserRequest(
            FirstName: "OtherName"
        );

        var httpRequest = new HttpRequestMessage(HttpMethod.Patch, "/api/users/me")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", sb.ToString());

        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

}