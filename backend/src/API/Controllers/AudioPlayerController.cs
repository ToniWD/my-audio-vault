using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public AudioPlayerController(ILogger<AudioPlayerController> logger, ISender mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<ActionResult<PagedResponse<SongDetailsDto>?>> GetSongs(SongQueryFilter filter)
    {
        var result = await _mediator.Send(filter);
        return Ok(result);
    }

    [HttpPost("/upload")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    [HttpGet("/{id}")]
    public async Task<ActionResult<SongDetailsDto>> GetSongById(string id)
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id}/stream")]
    public async Task<IActionResult> StreamSong(string id)
    {
        throw new NotImplementedException();
    }
}
