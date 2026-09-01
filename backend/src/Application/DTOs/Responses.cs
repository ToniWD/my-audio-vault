namespace MusicStream.Application.DTOs;

public record AuthenticationResponse(
    string AccessToken,
    DateTime AccessTokenExpirationTime,
    string RefreshToken
);