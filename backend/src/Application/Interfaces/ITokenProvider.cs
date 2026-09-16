using System.Security.Claims;
using MusicStream.Core.Entities;

namespace MusicStream.Application.Interfaces;

public interface ITokenProvider
{
    public (string, DateTime) GenerateAccessToken(User user);

    public string GenerateRefreshToken();

    public bool ValidateRefreshToken(string refreshToken);
}