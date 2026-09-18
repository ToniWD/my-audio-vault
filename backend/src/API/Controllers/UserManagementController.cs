using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStream.Application.DTOs;


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
}