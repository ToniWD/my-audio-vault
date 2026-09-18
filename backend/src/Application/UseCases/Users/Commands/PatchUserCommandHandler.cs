using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MusicStream.Application.DTOs;
using MusicStream.Application.Exceptions;
using MusicStream.Core.Entities;

namespace MusicStream.Application.UseCases;

public class PatchUserCommandHandler : IRequestHandler<PatchUserCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<PatchUserCommandHandler> _logger;

    public PatchUserCommandHandler(UserManager<User> userManager, ILogger<PatchUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task Handle(PatchUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            throw new NotFoundException($"User with ID {request.UserId} was not found.");
        }

        if (request.Email is not null && !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                throw new AppException($"Email '{request.Email}' is already taken.");
            }

            user.Email = request.Email;
            user.NormalizedEmail = _userManager.NormalizeEmail(request.Email);
        }

        if (request.Username is not null && !string.Equals(user.UserName, request.Username, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userManager.FindByNameAsync(request.Username);
            if (existingUser is not null)
            {
                throw new AppException($"Username '{request.Username}' is already taken.");
            }
            user.UserName = request.Username;
            user.NormalizedUserName = _userManager.NormalizeName(request.Username);
        }

        if (request.FirstName is not null)
        {
            user.FirstName = request.FirstName;
        }

        if (request.LastName is not null)
        {
            user.LastName = request.LastName;
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"Patch failed: {errors}");
            throw new AppException("Server error");
        }
    }
}