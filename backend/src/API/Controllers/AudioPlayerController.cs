using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MusicStream.Application.DTOs;
using MusicStream.Application.Exceptions;
using MusicStream.Application.UseCases;

[Route("api/songs")]
[ApiController]
[Authorize]
public class AudioPlayerController : ControllerBase
{
    private readonly ILogger<AudioPlayerController> _logger;
    private readonly ISender _mediator;
    private readonly IDistributedCache _cache;

    public AudioPlayerController(ILogger<AudioPlayerController> logger, ISender mediator, IDistributedCache cache)
    {
        _logger = logger;
        _mediator = mediator;
        _cache = cache;
    }

    [HttpGet("")]
    [ProducesResponseType(typeof(PagedResponse<SongDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<SongDetailsDto>?>> GetSongs([FromQuery] SongQueryFilter filter)
    {
        var result = await _mediator.Send(filter);
        return Ok(result);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<Guid>?> PostSong(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new AppException("No file provided.");

        await using var stream = file.OpenReadStream();

        var command = new UploadAudioFileCommand(
            FileStream: stream,
            FileName: file.FileName,
            ContentType: file.ContentType,
            Length: file.Length
        );

        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("stream/{token:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status206PartialContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StreamAudio(Guid token, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to stream audio with token: {Token}", token);
        var cacheKey = $"stream_token:{token}";

        var songIdString = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrEmpty(songIdString) || !Guid.TryParse(songIdString, out Guid songId))
        {
            return Unauthorized(new { message = "Invalid or expired token." });
        }

        // await _cache.RemoveAsync(cacheKey);

        string? rangeHeader = Request.Headers["Range"].FirstOrDefault();

        var query = new GetAudioStreamQuery(songId, rangeHeader);
        var result = await _mediator.Send(query, cancellationToken);

        await using var stream = result.Stream;

        // Advertise byte-range support together with a real Content-Length so
        // players treat the source as seekable instead of clamping seeks to 0.
        Response.Headers["Accept-Ranges"] = "bytes";
        Response.ContentType = result.ContentType;
        Response.ContentLength = result.ContentLength;

        if (string.IsNullOrEmpty(result.ContentRange))
        {
            Response.StatusCode = StatusCodes.Status200OK;
        }
        else
        {
            // Storage honoured the Range request; forward its exact bounds.
            Response.StatusCode = StatusCodes.Status206PartialContent;
            Response.Headers["Content-Range"] = result.ContentRange;
        }

        await stream.CopyToAsync(Response.Body, cancellationToken);

        return new EmptyResult();
    }

    [HttpPost("token/{songId:guid}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateToken(Guid songId)
    {
        var token = Guid.NewGuid().ToString();

        // Long enough to cover a whole track so buffering/seeking keeps working.
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };

        await _cache.SetStringAsync($"stream_token:{token}", songId.ToString(), options);

        return Ok(token);
    }

    [HttpGet("{songId:guid}")]
    [ProducesResponseType(typeof(SongDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SongDetailsDto>> GetSongById(Guid songId)
    {
        var query = new GetSongDetailsByIdQuery(songId);
        var result = await _mediator.Send(query);

        if (result is null)
            return NotFound();

        return Ok(result);
    }
}
