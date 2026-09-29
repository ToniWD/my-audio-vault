using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MusicStream.Application.Exceptions;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;
using MusicStream.Core.Enums;

namespace MusicStream.Application.UseCases;

public record DeleteUserCommand(Guid UserId, string? Password, bool ForceDeleteWithoutPassword = false) : IRequest;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _repo;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(UserManager<User> userManager, IUserRepository repository, ILogger<DeleteUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
        _repo = repository;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            throw new NotFoundException("User account not found.");
        }

        if (!request.ForceDeleteWithoutPassword)
        {
            if (string.IsNullOrWhiteSpace(request.Password) || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new AppException("Invalid password. Account deletion requires the current password.");
            }
        }

        if (user.Role == UserRole.Admin && await _repo.CheckHowManyAdminsExists([request.UserId]) == 0)
        {
            throw new Forbidden("Cannot delete this account because it is the sole administrator. Assign another administrator before deleting.");
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Code + ": " + e.Description));

            _logger.LogError("Failed to delete user {UserId}. Errors: {Errors}", request.UserId, errors);

            throw new AppException("An unexpected error occurred while deleting the user account.");
        }

        _logger.LogInformation("User account {UserId} was successfully deleted.", request.UserId);
    }
}