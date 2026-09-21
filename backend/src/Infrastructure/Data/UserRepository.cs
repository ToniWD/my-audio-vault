using Microsoft.EntityFrameworkCore;
using MusicStream.Application.DTOs;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;
using MusicStream.Core.Enums;

namespace MusicStream.Infrastructure.Data;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbDbContext)
    {
        _dbContext = dbDbContext;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<UserDTO?> GetProfileDtoByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserDTO(
                u.Id,
                u.UserName ?? string.Empty,
                u.FirstName,
                u.LastName,
                u.Email ?? string.Empty,
                u.Role
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid?> GetUserIdByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var userToken = await _dbContext.UserTokens
            .FirstOrDefaultAsync(ut =>
                ut.LoginProvider == "JWT" &&
                ut.Name == "RefreshToken" &&
                ut.Value == refreshToken,
                cancellationToken);

        return userToken?.UserId;
    }

    public async Task<int> CheckHowManyAdminsExists(List<Guid>? except = null)
    {
        var query = _dbContext.Users.AsNoTracking();

        query = query.Where(u => u.Role == UserRole.Admin);

        if (except is not null && except.Count > 0)
        {
            query = query.Where(u => !except.Contains(u.Id));
        }

        return await query.CountAsync();
    }
}