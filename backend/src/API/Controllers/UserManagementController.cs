using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicStream.Application.DTOs;

namespace MusicStream.API.Controllers;

[Route("api/auth")]
[ApiController]
public class UserManagementController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(ILogger<UserManagementController> logger, ISender mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var userId = await _mediator.Send(registerRequest);
        return Ok(new { UserId = userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        return Ok();
    }
}