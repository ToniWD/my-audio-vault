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

    [HttpGet("{songId:guid}/presigned-url")]
    [ProducesResponseType(typeof(PresignedUrl), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PresignedUrl>> GetPresignedUrl(Guid songId)
    {
        var query = new GenerateSongPresignedUrlCommand(songId);
        var result = await _mediator.Send(query);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{songId:guid}/cover-image/presigned-url")]
    [ProducesResponseType(typeof(PresignedUrl), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PresignedUrl>> GetPresignedUrlCoverImage(Guid songId)
    {
        var query = new GenerateImagePresignedUrlCommand(songId);
        var result = await _mediator.Send(query);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{songId:guid}/cover-image")]
    [ProducesResponseType(typeof(PresignedUrl), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PresignedUrl>> UploadCoverImage(Guid songId, IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new AppException("No file provided.");

        await using var stream = file.OpenReadStream();

        var command = new UploadCoverImageCommand(
            SongId: songId,
            FileStream: stream,
            FileName: file.FileName,
            ContentType: file.ContentType,
            Length: file.Length
        );

        var result = await _mediator.Send(command);

        return Ok(result);
    }
}
