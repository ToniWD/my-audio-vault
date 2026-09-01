using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MusicStream.Application.DTOs;
using MusicStream.Application.Exceptions;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;

namespace MusicStream.Application.UseCases;

public class LoginUserHandler : IRequestHandler<LoginRequest, AuthenticationResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenProvider _tokenProviderHelper;
    private readonly ILogger<LoginUserHandler> _logger;

    public LoginUserHandler(UserManager<User> userManager, ITokenProvider tokenProvider, ILogger<LoginUserHandler> logger
    )
    {
        _userManager = userManager;
        _logger = logger;
        _tokenProviderHelper = tokenProvider;
    }

    public async Task<AuthenticationResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(request.ToString());
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var result = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!result)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var (token, expiration) = _tokenProviderHelper.GenerateAccessToken(user);
        var refreshToken = _tokenProviderHelper.GenerateRefreshToken();

        // TODO: Update refresh token in db

        return new AuthenticationResponse(
            token,
            expiration,
            refreshToken
        );
    }
}