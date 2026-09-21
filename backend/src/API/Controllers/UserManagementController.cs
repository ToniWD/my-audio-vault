using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MusicStream.Application.DTOs;
using MusicStream.Application.UseCases;
using MusicStream.Core.Enums;


namespace MusicStream.API.Controllers;

[Route("api/users")]
[ApiController]
[Authorize]
public class UserManagementController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(ILogger<UserManagementController> logger, ISender mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDTO>> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var data = await _mediator.Send(new GetUserProfileByIdQuery(userId));

        return Ok(data);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDTO>> GetProfileById(Guid userId)
    {
        var data = await _mediator.Send(new GetUserProfileByIdQuery(userId));

        return Ok(data);
    }

    [HttpPatch("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> PatchProfile([FromBody] PatchUserRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var command = new PatchUserCommand(
                UserId: userId,
                Username: request.Username,
                FirstName: request.FirstName,
                LastName: request.LastName,
                Email: request.Email
            );

        await _mediator.Send(command);

        return new NoContentResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserProfile(Guid userId, [FromBody] PatchUserRequest request)
    {
        var command = new PatchUserCommand(
            UserId: userId,
            Username: request.Username,
            FirstName: request.FirstName,
            LastName: request.LastName,
            Email: request.Email
        );

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProfile([FromBody] DeleteAccountRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var command = new DeleteUserCommand(
                UserId: userId,
                Password: request.Password
            );

        await _mediator.Send(command);

        return new NoContentResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUserById(Guid userId)
    {
        var command = new DeleteUserCommand(
            UserId: userId,
            Password: null,
            ForceDeleteWithoutPassword: true
        );

        await _mediator.Send(command);

        return NoContent();
    }
}