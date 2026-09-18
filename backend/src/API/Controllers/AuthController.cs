using MediatR;
using Microsoft.AspNetCore.Mvc;
using MusicStream.Application.DTOs;

namespace MusicStream.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger, ISender mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest registerRequest)
    {
        var userId = await _mediator.Send(registerRequest);
        return Created(string.Empty, new RegisterResponse(userId));
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }

    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<HealthCheck> HealthCheck()
    {
        return Ok(new HealthCheck("Healthy", DateTime.UtcNow));
    }
}