using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MusicStream.API.Configuration;
using MusicStream.Application.DTOs;
using MusicStream.Core.Entities;
using MusicStream.Core.Enums;

namespace MusicStream.API.Controllers;

[Route("api/auth")]
[ApiController]
public class UserManagementController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly AuthenticationSettings _authSettings;

    public UserManagementController(UserManager<User> userManager, IOptions<AuthenticationSettings> authSettings)
    {
        _userManager = userManager;
        _authSettings = authSettings.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        // Fluid auto validation already checks for not null fields in 'registerRequest'
        // Check if user exists 
        var userExists = await _userManager.FindByEmailAsync(registerRequest.Email!);
        if (userExists is not null)
        {
            return BadRequest(new { Message = "This email address is already in use." });
        }

        userExists = await _userManager.FindByNameAsync(registerRequest.Username!);
        if (userExists is not null)
        {
            return BadRequest(new { Message = "This username is already taken." });
        }

        var user = new User
        {
            UserName = registerRequest.Username,
            FirstName = registerRequest.FirstName!,
            LastName = registerRequest.LastName!,
            Email = registerRequest.Email,
            Role = UserRole.Member
        };

        // Create user
        var result = await _userManager.CreateAsync(user, registerRequest.Password!);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return StatusCode(201, new { Message = "User registered successfully!" });
    }
}