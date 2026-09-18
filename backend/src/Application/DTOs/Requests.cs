using MediatR;

namespace MusicStream.Application.DTOs;

public record RegisterRequest(
    string? Username,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Password
) : IRequest<Guid>;

public record PatchUserRequest(
    string? Username = null,
    string? FirstName = null,
    string? LastName = null,
    string? Email = null
);

public record PatchUserCommand(
    Guid UserId,
    string? Username = null,
    string? FirstName = null,
    string? LastName = null,
    string? Email = null
) : IRequest;

public record LoginRequest(
    string Email,
    string Password
) : IRequest<AuthenticationResponse>;

public record RefreshTokenRequest(
    string RefreshToken
) : IRequest<AuthenticationResponse>;