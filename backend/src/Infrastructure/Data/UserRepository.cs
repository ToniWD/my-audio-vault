using Microsoft.EntityFrameworkCore;
using MusicStream.Application.Interfaces;

namespace MusicStream.Infrastructure.Data;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbDbContext)
    {
        _dbContext = dbDbContext;
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
}