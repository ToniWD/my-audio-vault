using MusicStream.Core.Entities;

namespace MusicStream.Application.Interfaces;

public interface IUserRepository
{
    Task<Guid?> GetUserIdByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}