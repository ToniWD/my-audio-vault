using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;
using MusicStream.Core.Enums;

namespace MusicStream.IntegrationTests;

public static class UserUtils
{
    /// <summary>
    /// Gets a pre-existing test user or creates one.
    /// This user data should not be changed.
    /// </summary>
    public static async Task<(User User, string Password)> CreateUserDB(CustomWebApplicationFactory factory, UserRole role = UserRole.Member)
    {
        User user = new User();


        string email = $"user_{Guid.NewGuid():N}@gen.com";
        string password = $"Password123!{Guid.NewGuid():N}";
        string username = $"us_{Guid.NewGuid():N}";

        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is null) existingUser = await userManager.FindByNameAsync(email);

        if (existingUser is not null) throw new Exception("UserUtils can't create a new user");

        var newUser = new User
        {
            UserName = username,
            Email = email,
            FirstName = "FnameGen",
            LastName = "LnameGen",
            Role = role
        };

        var createResult = await userManager.CreateAsync(newUser, password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not create test user '{username}': {errors}");
        }

        return (newUser, password);
    }

    public static async Task<string> GetAccessTokenForExistingUser(string userId, CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var existingUser = await userManager.FindByIdAsync(userId);

        if (existingUser is null)
        {
            throw new Exception("No user found.");
        }

        var tokenProviderHelper = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
        var (token, _) = tokenProviderHelper.GenerateAccessToken(existingUser);
        return token;

    }

    public static async Task<string> GetRefreshTokenForExistingUser(string userId, CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var existingUser = await userManager.FindByIdAsync(userId);

        if (existingUser is null)
        {
            throw new Exception("No user found.");
        }

        string? storedToken = await userManager.GetAuthenticationTokenAsync(
            existingUser,
            loginProvider: "JWT",
            tokenName: "RefreshToken"
        );

        if (string.IsNullOrEmpty(storedToken))
        {
            var tokenProviderHelper = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
            storedToken = tokenProviderHelper.GenerateRefreshToken();
            var result = await userManager.SetAuthenticationTokenAsync(
                existingUser,
                loginProvider: "JWT",
                tokenName: "RefreshToken",
                tokenValue: storedToken
            );

            if (!result.Succeeded)
            {
                throw new InvalidOperationException("The refresh token can't be saved.");
            }
        }

        return storedToken;
    }
    public static async Task<User?> GetExistingUser(string userId, CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var existingUser = await userManager.FindByIdAsync(userId);

        return existingUser;
    }

    public static async Task DeleteExistingUser(User user, CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        await userManager.DeleteAsync(user);
    }
}