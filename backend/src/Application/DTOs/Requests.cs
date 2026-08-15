namespace MusicStream.Application.DTOs;

public record RegisterRequest(
    string? Username,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Password
);