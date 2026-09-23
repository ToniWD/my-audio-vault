using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MusicStream.Application.DTOs;
using MusicStream.Core.Entities;
using MusicStream.Core.Enums;

namespace MusicStream.IntegrationTests.Tests;

// TODO
[Collection("IntegrationTestWithDBCollection")]
public class UserManagementAdminTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ILogger<UserManagementAdminTests> _logger;

    // User is static because all test from this class (collection) work in sequential order,
    // but for every test a new instance of this class is created 
    private static User _sharedAdmin;
    private static string _sharedAdminPassword;

    public UserManagementAdminTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<UserManagementAdminTests>();
    }
    public async ValueTask InitializeAsync()
    {
        if (_sharedAdmin is null)
        {
            var user = await UserUtils.GetExistingUserByUsername("admin", _factory);

            if (user is not null && user.Role == UserRole.Admin)
            {
                await UserUtils.DeleteExistingUser(user, _factory);
            }
            (_sharedAdmin, _sharedAdminPassword) = await UserUtils.CreateUserDB(_factory, UserRole.Admin);
            _logger.LogInformation($"New admin created for {this.GetType().Name} with id: {_sharedAdmin.Id}");
        }
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task GetProfile_OfSelf_AsAdmin_ReturnsOk()
    {
        var accessToken = await UserUtils.GetAccessTokenForExistingUser(_sharedAdmin.Id.ToString(), _factory);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{_sharedAdmin.Id}");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var userProfile = await response.Content.ReadFromJsonAsync<UserDTO>();

        Assert.NotNull(userProfile);
        Assert.Equal(_sharedAdmin.Id, userProfile.Id);
        Assert.Equal(_sharedAdmin.UserName, userProfile.Username);
        Assert.Equal(_sharedAdmin.Email, userProfile.Email);
        Assert.Equal(_sharedAdmin.FirstName, userProfile.FirstName);
        Assert.Equal(_sharedAdmin.LastName, userProfile.LastName);
        Assert.Equal(_sharedAdmin.Role, userProfile.Role);
    }

    [Fact]
    public async Task GetProfile_OfAnotherUser_AsAdmin_ReturnsOk()
    {
        var (user, _) = await UserUtils.CreateUserDB(_factory);
        try
        {
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(_sharedAdmin.Id.ToString(), _factory);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{user.Id}");

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
    public async Task GetProfile_OfAnotherUser_WithMemberToken_ReturnsForbidden()
    {
        var (user, _) = await UserUtils.CreateUserDB(_factory);
        var (user2, _) = await UserUtils.CreateUserDB(_factory);
        try
        {
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(user.Id.ToString(), _factory);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{user2.Id}");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _client.SendAsync(httpRequest);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            var existingUser1 = await UserUtils.GetExistingUser(user.Id.ToString(), _factory);
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
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(_sharedAdmin.Id.ToString(), _factory);

            var request = new PatchUserRequest(
                Username: username,
                FirstName: firstName,
                LastName: lastName,
                Email: email
            );

            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/users/{testUser.Id}")
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
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(_sharedAdmin.Id.ToString(), _factory);

            var request = new PatchUserRequest(
                Username: username,
                FirstName: firstName,
                LastName: lastName,
                Email: email
            );

            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/users/{testUser.Id}")
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
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(_sharedAdmin.Id.ToString(), _factory);
            StringBuilder sb = new StringBuilder(accessToken);

            if (sb[32] != 'c') sb[32] += 'c';
            else sb[32] = 'a';

            var request = new PatchUserRequest(
                FirstName: "OtherName"
            );

            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/users/{testUser.Id}")
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
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(_sharedAdmin.Id.ToString(), _factory);

            var request = new PatchUserRequest(
                Username: user2.UserName
            );

            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/users/{user1.Id}")
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
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(_sharedAdmin.Id.ToString(), _factory);

            var request = new PatchUserRequest(
                Email: user2.Email
            );

            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/users/{user1.Id}")
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
    public async Task PatchProfile_WithMemberToken_ReturnsForbidden()
    {
        var (user1, _) = await UserUtils.CreateUserDB(_factory);
        var (user2, _) = await UserUtils.CreateUserDB(_factory);

        try
        {
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(user2.Id.ToString(), _factory);

            var request = new PatchUserRequest(
                FirstName: "OtherName"
            );

            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/users/{user1.Id}")
            {
                Content = JsonContent.Create(request)
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Act
            var response = await _client.SendAsync(httpRequest);

            // Assert StatusCode
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            var updatedUser = await UserUtils.GetExistingUser(user1.Id.ToString(), _factory);

            Assert.NotNull(updatedUser);
            Assert.Equal(user1.FirstName, updatedUser.FirstName);
            Assert.NotEqual("OtherName", user1.FirstName);
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

    [Theory]
    [InlineData("api/users/me", false)]
    [InlineData("api/users/", true)]
    public async Task DeleteAccount_OnlyOneAdmin_ReturnsForbidden(string route, bool addId)
    {
        var accessToken = await UserUtils.GetAccessTokenForExistingUser(_sharedAdmin.Id.ToString(), _factory);

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, route + (addId ? _sharedAdmin.Id.ToString() : ""));

        if (!addId)
        {
            var request = new DeleteAccountRequest(
                Password: _sharedAdminPassword
            );
            httpRequest.Content = JsonContent.Create(request);
        }

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var response = await _client.SendAsync(httpRequest);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            Assert.NotNull(await UserUtils.GetExistingUser(_sharedAdmin.Id.ToString(), _factory));
        }
        finally
        {
            var existingUser = await UserUtils.GetExistingUser(_sharedAdmin.Id.ToString(), _factory);
            if (existingUser == null)
            {
                (_sharedAdmin, _sharedAdminPassword) = await UserUtils.CreateUserDB(_factory, UserRole.Admin);
            }
        }
    }


    [Theory]
    [InlineData("api/users/me", false)]
    [InlineData("api/users/", true)]
    public async Task DeleteAccount_WhenAnotherAdminExists_ReturnsNoContent(string route, bool addId)
    {
        var (otherAdmin, password) = await UserUtils.CreateUserDB(_factory, UserRole.Admin);
        var accessToken = await UserUtils.GetAccessTokenForExistingUser(otherAdmin.Id.ToString(), _factory);

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, route + (addId ? otherAdmin.Id.ToString() : ""));

        if (!addId)
        {
            var request = new DeleteAccountRequest(
                Password: password
            );
            httpRequest.Content = JsonContent.Create(request);
        }


        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var response = await _client.SendAsync(httpRequest);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            Assert.Null(await UserUtils.GetExistingUser(otherAdmin.Id.ToString(), _factory));
            Assert.NotNull(await UserUtils.GetExistingUser(_sharedAdmin.Id.ToString(), _factory));
        }
        finally
        {
            var existingUser = await UserUtils.GetExistingUser(otherAdmin.Id.ToString(), _factory);
            if (existingUser != null)
            {
                await UserUtils.DeleteExistingUser(otherAdmin, _factory);
            }
        }
    }

    [Fact]
    public async Task DeleteAccount_OfExistingMember_ReturnsNoContent()
    {
        var (user, _) = await UserUtils.CreateUserDB(_factory, UserRole.Member);
        var accessToken = await UserUtils.GetAccessTokenForExistingUser(_sharedAdmin.Id.ToString(), _factory);

        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/users/{user.Id}");

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var response = await _client.SendAsync(httpRequest);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            Assert.Null(await UserUtils.GetExistingUser(user.Id.ToString(), _factory));
        }
        finally
        {
            var existingUser = await UserUtils.GetExistingUser(user.Id.ToString(), _factory);
            if (existingUser != null)
            {
                await UserUtils.DeleteExistingUser(user, _factory);
            }
        }
    }

    [Fact]
    public async Task DeleteAccount_OfNonExistingMember_ReturnsNoContent()
    {
        var accessToken = await UserUtils.GetAccessTokenForExistingUser(_sharedAdmin.Id.ToString(), _factory);

        var id = Guid.NewGuid();
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/users/{id}");

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(await UserUtils.GetExistingUser(id.ToString(), _factory));
    }

    [Fact]
    public async Task DeleteAccount_OfAnotherUser_WithMemberToken_ReturnsForbidden()
    {
        var (user, _) = await UserUtils.CreateUserDB(_factory, UserRole.Member);
        var (user2, _) = await UserUtils.CreateUserDB(_factory, UserRole.Member);
        try
        {
            var accessToken = await UserUtils.GetAccessTokenForExistingUser(user.Id.ToString(), _factory);

            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/users/{user2.Id}");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _client.SendAsync(httpRequest);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotNull(await UserUtils.GetExistingUser(user.Id.ToString(), _factory));
            Assert.NotNull(await UserUtils.GetExistingUser(user2.Id.ToString(), _factory));
        }
        finally
        {
            var existingUser1 = await UserUtils.GetExistingUser(user.Id.ToString(), _factory);
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

}