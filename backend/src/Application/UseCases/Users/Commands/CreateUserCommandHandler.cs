using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MusicStream.Application.DTOs;
using MusicStream.Application.Exceptions;
using MusicStream.Core.Entities;
using MusicStream.Core.Enums;

namespace MusicStream.Application.UseCases;

public class CreateUserCommandHandler : IRequestHandler<RegisterRequest, Guid>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(UserManager<User> userManager, ILogger<CreateUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Guid> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        // Check if user exists 
        var userByEmail = await _userManager.FindByEmailAsync(request.Email!);
        if (userByEmail is not null)
        {
            errors.Add("This email address is already in use.");
        }

        var userByUsername = await _userManager.FindByNameAsync(request.Username!);
        if (userByUsername is not null)
        {
            errors.Add("This username is already taken.");
        }

        if (errors.Count != 0)
        {
            throw new ValidationException(string.Join("\n", errors));
        }

        var user = new User
        {
            UserName = request.Username,
            FirstName = request.FirstName!,
            LastName = request.LastName!,
            Email = request.Email,
            Role = UserRole.Member
        };

        var result = await _userManager.CreateAsync(user, request.Password!);

        if (!result.Succeeded)
        {
            var identityErrors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("User registration failed for {Email}. Errors: {Errors}", request.Email, identityErrors);

            throw new AppException($"Registration failed: {identityErrors}");
        }

        return user.Id;
    }
}