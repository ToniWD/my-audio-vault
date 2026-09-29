using MusicStream.Core.Enums;

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

public record UserDTO(
    Guid Id,
    string Username,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role
);

public record AudioFileSummaryDto(
    Guid Id,
    string Format,         // ex: "mp3", "aac", "flac"
    string Quality,        // ex: "Low", "Medium", "High", "Lossless"
    int BitrateKbps,
    long FileSizeBytes
);

public record SongDetailsDto(
    Guid Id,
    string Title,
    string CoverImageUrl,
    string Genre,
    DateOnly ReleaseDate,
    bool IsExplicit,
    int DurationSeconds,
    IReadOnlyCollection<AudioFileSummaryDto> AvailableQualities
);