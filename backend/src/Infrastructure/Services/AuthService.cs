// using Microsoft.EntityFrameworkCore;
// using MusicStream.Application.DTOs;
// using MusicStream.Application.Interfaces;
// using MusicStream.Infrastructure.Data;

// namespace MusicStream.Application.Services;

// public class AuthService(
//     AppDbContext appDbContext
// ) : IAuthService
// {
//     public async Task<AuthResponseDto?> Login(UserCredentialDto credential)
//     {
//         var user = await appDbContext.Users
//             .FirstOrDefaultAsync(u => u.Username == credential.Username);

//         if (user is null)
//         {
//             throw new Exception("Incorrect username or password");
//         }



//         return new AuthResponseDto()

//     }
// }