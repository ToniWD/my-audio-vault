namespace MusicStream.Application.DTOs;

public record AuthenticationResponse(
    string AccessToken,
    DateTime AccessTokenExpirationTime,
    string RefreshToken
);

public record HealthCheck(
    string Status,
    DateTime TimeStamp
);

public record RegisterResponse(Guid UserId);