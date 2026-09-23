using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;
using MusicStream.Core.Enums;

namespace MusicStream.Application.UseCases;

public record SeedFirstAdminCommand(
    string Username = "admin",
    string? Password = null
) : IRequest<UserCredentials?>;

public record UserCredentials(
    string Username,
    string Password
);

public class SeedFirstAdminCommandHandler : IRequestHandler<SeedFirstAdminCommand, UserCredentials?>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<SeedFirstAdminCommandHandler> _logger;
    private readonly IUserRepository _userRepo;

    public SeedFirstAdminCommandHandler(
        UserManager<User> userManager,
        IUserRepository repository,
        ILogger<SeedFirstAdminCommandHandler> logger)
    {
        _userManager = userManager;
        _userRepo = repository;
        _logger = logger;
    }

    public async Task<UserCredentials?> Handle(SeedFirstAdminCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepo.CheckHowManyAdminsExists() > 0)
        {
            return null;
        }

        var password = request.Password ?? $"{Guid.NewGuid().ToString("N")[..10]}!Admin1";

        var user = new User
        {
            UserName = request.Username,
            Email = $"{request.Username}@musicstream.local",
            Role = UserRole.Admin
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to seed initial admin user. Errors: {Errors}", errors);

            throw new InvalidOperationException($"Could not seed initial admin: {errors}");
        }

        _logger.LogWarning("Initial admin user successfully created.");

        return new UserCredentials(request.Username, password);
    }
}