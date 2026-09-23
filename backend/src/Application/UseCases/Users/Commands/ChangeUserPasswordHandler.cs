using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MusicStream.Application.Exceptions;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;

namespace MusicStream.Application.UseCases;

public record ChangeUserPasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest;


public class ChangeUserPasswordHandler : IRequestHandler<ChangeUserPasswordCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _repo;
    private readonly ILogger<ChangeUserPasswordHandler> _logger;

    public ChangeUserPasswordHandler(UserManager<User> userManager, IUserRepository repository, ILogger<ChangeUserPasswordHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
        _repo = repository;
    }

    public async Task Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException("User account not found.");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var identityErrors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("The password for user {UserName} couldn't be changed. Errors: {Errors}", user.UserName, identityErrors);

            throw new AppException($"Password change failed: {identityErrors}");
        }

        await _userManager.SetAuthenticationTokenAsync(
            user,
            loginProvider: "JWT",
            tokenName: "RefreshToken",
            tokenValue: null
        );
    }
}