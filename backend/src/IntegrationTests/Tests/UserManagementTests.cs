using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MusicStream.Application.DTOs;

namespace MusicStream.IntegrationTests.Tests;

[Collection("IntegrationTestWithDBCollection")]
public class UserManagementTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ILogger<UserManagementTests> _logger;

    public UserManagementTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<UserManagementTests>();
    }

    [Theory]
    [InlineData("alex12", "Alex", "Jobs", "alex12test@example.com")]
    [InlineData("onlyUserName", null, null, null)]
    [InlineData(null, "OnlyFirstName", null, null)]
    [InlineData(null, null, "OnlyLastName", null)]
    [InlineData(null, null, null, "onlyemail@example.com")]
    [InlineData("Username", null, "LastName", null)]
    public async Task PatchProfile_WithValidData_ReturnsNoContent(string? username, string? firstName, string? lastName, string? email)
    {
        var (testUser, _) = await UserUtils.CreateUserDB(_factory);

        try
        {
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

            Assert.NotNull(updatedUser);

            Assert.Equal(username ?? testUser.UserName, updatedUser.UserName);
            Assert.Equal(firstName ?? testUser.FirstName, updatedUser.FirstName);
            Assert.Equal(lastName ?? testUser.LastName, updatedUser.LastName);
            Assert.Equal(email ?? testUser.Email, updatedUser.Email);
        }
        finally
        {
            var existingUser = await UserUtils.GetExistingUser(testUser.Id.ToString(), _factory);
            if (existingUser is not null)
            {
                await UserUtils.DeleteExistingUser(existingUser, _factory);
            }
        }
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
    public async Task PatchProfile_WithInvalidData_ReturnsBadRequest(string? username, string? firstName, string? lastName, string? email)
    {
        var (testUser, _) = await UserUtils.CreateUserDB(_factory);

        try
        {
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

            Assert.NotNull(updatedUser);
            Assert.Equal(testUser.UserName, updatedUser.UserName);
            Assert.Equal(testUser.FirstName, updatedUser.FirstName);
            Assert.Equal(testUser.LastName, updatedUser.LastName);
            Assert.Equal(testUser.Email, updatedUser.Email);
        }
        finally
        {
            var existingUser = await UserUtils.GetExistingUser(testUser.Id.ToString(), _factory);
            if (existingUser is not null)
            {
                await UserUtils.DeleteExistingUser(existingUser, _factory);
            }
        }
    }

    [Fact]
    public async Task PatchProfile_WithInvalidAccessToken_ReturnsUnauthorized()
    {
        var (testUser, _) = await UserUtils.CreateUserDB(_factory);
        try
        {
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(testUser.Id.ToString(), _factory);
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
        finally
        {
            var existingUser = await UserUtils.GetExistingUser(testUser.Id.ToString(), _factory);
            if (existingUser is not null)
            {
                await UserUtils.DeleteExistingUser(existingUser, _factory);
            }
        }
    }

    [Fact]
    public async Task PatchProfile_WithExistingUsername_ReturnsBadRequest()
    {
        var (user1, _) = await UserUtils.CreateUserDB(_factory);
        var (user2, _) = await UserUtils.CreateUserDB(_factory);

        try
        {
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(user1.Id.ToString(), _factory);

            var request = new PatchUserRequest(
                Username: user2.UserName
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

            var updatedUser = await UserUtils.GetExistingUser(user1.Id.ToString(), _factory);

            Assert.NotNull(updatedUser);
            Assert.Equal(user1.UserName, updatedUser.UserName);
            Assert.NotEqual(user2.UserName, updatedUser.UserName);
        }
        finally
        {
            var existingUser1 = await UserUtils.GetExistingUser(user1.Id.ToString(), _factory);
            if (existingUser1 is not null)
            {
                await UserUtils.DeleteExistingUser(existingUser1, _factory);
            }

            var existingUser2 = await UserUtils.GetExistingUser(user2.Id.ToString(), _factory);
            if (existingUser2 is not null)
            {
                await UserUtils.DeleteExistingUser(existingUser2, _factory);
            }
        }
    }

    [Fact]
    public async Task PatchProfile_WithExistingEmail_ReturnsBadRequest()
    {
        var (user1, _) = await UserUtils.CreateUserDB(_factory);
        var (user2, _) = await UserUtils.CreateUserDB(_factory);

        try
        {
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(user1.Id.ToString(), _factory);

            var request = new PatchUserRequest(
                Email: user2.Email
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

            var updatedUser = await UserUtils.GetExistingUser(user1.Id.ToString(), _factory);

            Assert.NotNull(updatedUser);
            Assert.Equal(user1.Email, updatedUser.Email);
            Assert.NotEqual(user2.Email, updatedUser.Email);
        }
        finally
        {
            var existingUser1 = await UserUtils.GetExistingUser(user1.Id.ToString(), _factory);
            if (existingUser1 is not null)
            {
                await UserUtils.DeleteExistingUser(existingUser1, _factory);
            }

            var existingUser2 = await UserUtils.GetExistingUser(user2.Id.ToString(), _factory);
            if (existingUser2 is not null)
            {
                await UserUtils.DeleteExistingUser(existingUser2, _factory);
            }
        }
    }


    [Fact]
    public async Task GetProfile_OfSelf_WithValidToken_ReturnsOk()
    {
        var (user, _) = await UserUtils.CreateUserDB(_factory);
        try
        {
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(user.Id.ToString(), _factory);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _client.SendAsync(httpRequest);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var userProfile = await response.Content.ReadFromJsonAsync<UserDTO>();

            Assert.NotNull(userProfile);
            Assert.Equal(user.Id, userProfile.Id);
            Assert.Equal(user.UserName, userProfile.Username);
            Assert.Equal(user.Email, userProfile.Email);
            Assert.Equal(user.FirstName, userProfile.FirstName);
            Assert.Equal(user.LastName, userProfile.LastName);
            Assert.Equal(user.Role, userProfile.Role);
        }
        finally
        {
            var existingUser = await UserUtils.GetExistingUser(user.Id.ToString(), _factory);
            if (existingUser is not null)
            {
                await UserUtils.DeleteExistingUser(existingUser, _factory);
            }
        }
    }

    [Fact]
    public async Task GetProfile_OfSelf_WithInvalidToken_ReturnsUnauthorized()
    {
        var (user, _) = await UserUtils.CreateUserDB(_factory);
        try
        {
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(user.Id.ToString(), _factory);
            StringBuilder sb = new StringBuilder(accessToken);

            if (sb[32] != 'c') sb[32] += 'c';
            else sb[32] = 'a';

            var request = new PatchUserRequest(
                FirstName: "OtherName"
            );

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/api/users/me")
            {
                Content = JsonContent.Create(request)
            };

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", sb.ToString());

            var response = await _client.SendAsync(httpRequest);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        finally
        {
            var existingUser = await UserUtils.GetExistingUser(user.Id.ToString(), _factory);
            if (existingUser is not null)
            {
                await UserUtils.DeleteExistingUser(existingUser, _factory);
            }
        }
    }

    [Fact]
    public async Task DeleteAccount_WithValidPassword_ReturnsNoContent()
    {
        var (user, password) = await UserUtils.CreateUserDB(_factory);

        try
        {
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(user.Id.ToString(), _factory);

            var request = new DeleteAccountRequest(
                Password: password
            );

            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "api/users/me")
            {
                Content = JsonContent.Create(request)
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _client.SendAsync(httpRequest);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            Assert.Null(await UserUtils.GetExistingUser(user.Id.ToString(), _factory));
        }
        finally
        {
            var existingUser = await UserUtils.GetExistingUser(user.Id.ToString(), _factory);
            if (existingUser is not null)
            {
                await UserUtils.DeleteExistingUser(existingUser, _factory);
            }
        }
    }


    [Fact]
    public async Task DeleteAccount_WithInvalidPassword_ReturnsBadRequest()
    {
        var (user, password) = await UserUtils.CreateUserDB(_factory);

        try
        {
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(user.Id.ToString(), _factory);

            var request = new DeleteAccountRequest(
                Password: $"{password}a"
            );

            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "api/users/me")
            {
                Content = JsonContent.Create(request)
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _client.SendAsync(httpRequest);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            Assert.NotNull(await UserUtils.GetExistingUser(user.Id.ToString(), _factory));
        }
        finally
        {
            var existingUser = await UserUtils.GetExistingUser(user.Id.ToString(), _factory);
            if (existingUser is not null)
            {
                await UserUtils.DeleteExistingUser(existingUser, _factory);
            }
        }
    }
}