using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MusicStream.Application.Configuration;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;

namespace MusicStream.Infrastructure.Identity;

public class TokenProviderHelper : ITokenProvider
{
    private readonly AuthenticationSettings _authSettings;
    private readonly ILogger<TokenProviderHelper> _logger;

    public TokenProviderHelper(IOptions<AuthenticationSettings> authSettings, ILogger<TokenProviderHelper> logger)
    {
        _authSettings = authSettings.Value;
        _logger = logger;
    }

    public (string, DateTime) GenerateAccessToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_authSettings.TokenSecret));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expirationDate = DateTime.UtcNow.AddMinutes(15);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var jwtSecurityToken = new JwtSecurityToken(
            _authSettings.Issuer,
            _authSettings.Audience,
            claims,
            DateTime.UtcNow,
            expirationDate,
            signingCredentials
        );

        var tokenToReturn = new JwtSecurityTokenHandler()
            .WriteToken(jwtSecurityToken);

        return (tokenToReturn, expirationDate);
    }

    public string GenerateRefreshToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_authSettings.RefreshTokenSecret));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expirationDate = DateTime.UtcNow.AddHours(12);

        var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var jwtSecurityToken = new JwtSecurityToken(
            _authSettings.Issuer,
            _authSettings.Audience,
            claims,
            DateTime.UtcNow,
            expirationDate,
            signingCredentials
        );

        var tokenToReturn = new JwtSecurityTokenHandler()
           .WriteToken(jwtSecurityToken);

        return tokenToReturn;
    }

    public bool ValidateRefreshToken(string refreshToken)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_authSettings.RefreshTokenSecret));

        var validationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = securityKey,
            ValidIssuer = _authSettings.Issuer,
            ValidAudience = _authSettings.Audience,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var result = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.StackTrace);
            return false;
        }

    }
}