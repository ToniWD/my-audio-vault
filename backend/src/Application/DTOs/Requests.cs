using MediatR;

namespace MusicStream.Application.DTOs;

public record RegisterRequest(
    string? Username,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Password
) : IRequest<Guid>;

public record LoginRequest(
    string Email,
    string Password
) : IRequest<AuthenticationResponse>;