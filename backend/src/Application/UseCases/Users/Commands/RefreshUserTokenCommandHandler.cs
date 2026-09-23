using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MusicStream.Application.DTOs;
using MusicStream.Application.Exceptions;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;

namespace MusicStream.Application.UseCases;

public class RefreshUserToken : IRequestHandler<RefreshTokenRequest, AuthenticationResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenProvider _tokenProviderHelper;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RefreshUserToken> _logger;

    public RefreshUserToken(UserManager<User> userManager, ITokenProvider tokenProvider, IUserRepository userRepository, ILogger<RefreshUserToken> logger)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _logger = logger;
        _tokenProviderHelper = tokenProvider;
    }

    public async Task<AuthenticationResponse> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting token refresh.");

        if (!_tokenProviderHelper.ValidateRefreshToken(request.RefreshToken))
        {
            _logger.LogInformation("Invalid or expired refresh token.");
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        var userId = await _userRepository.GetUserIdByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (userId is null)
        {
            _logger.LogInformation("No user was found with this refresh token.");
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user is null)
        {
            _logger.LogInformation($"No user was found with the id {userId}.");
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        var storedRefreshToken = await _userManager.GetAuthenticationTokenAsync(
            user,
            loginProvider: "JWT",
            tokenName: "RefreshToken"
        );

        if (storedRefreshToken != request.RefreshToken)
        {
            _logger.LogWarning($"Refresh token mismatch for user {user.Id}. Token might have been revoked or reused.");

            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        var (newToken, expiration) = _tokenProviderHelper.GenerateAccessToken(user);
        var newRefreshToken = _tokenProviderHelper.GenerateRefreshToken();

        await _userManager.SetAuthenticationTokenAsync(
            user,
            loginProvider: "JWT",
            tokenName: "RefreshToken",
            tokenValue: newRefreshToken
        );

        return new AuthenticationResponse(
            newToken,
            expiration,
            newRefreshToken
        );
    }
}