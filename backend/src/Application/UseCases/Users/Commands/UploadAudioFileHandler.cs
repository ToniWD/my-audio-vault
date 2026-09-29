
using MediatR;
using Microsoft.Extensions.Logging;
using MusicStream.Application.Exceptions;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;

namespace MusicStream.Application.UseCases;

public record UploadAudioFileCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long Length
) : IRequest<Guid>;

public class UploadAudioFile : IRequestHandler<UploadAudioFileCommand, Guid>
{
    private readonly ISongRepository _repo;
    private readonly ILogger<UploadAudioFile> _logger;

    public UploadAudioFile(ISongRepository repository, ILogger<UploadAudioFile> logger)
    {
        _repo = repository;
        _logger = logger;
    }

    public async Task<Guid> Handle(UploadAudioFileCommand request, CancellationToken cancellationToken)
    {
        const long maxFileSize = 20 * 1024 * 1024; // max size 20 MB
        if (request.FileStream.Length > maxFileSize)
        {
            throw new AppException($"File size exceeds the maximum allowed limit of 20 MB.");
        }

        // File extension & MIME type validation
        var allowedExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".flac" };
        var allowedMimeTypes = new[] { "audio/mpeg", "audio/wav", "audio/ogg", "audio/x-m4a", "audio/aac", "audio/flac" };

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            throw new AppException("The provided file extension is not supported.");
        }

        if (!allowedMimeTypes.Contains(request.ContentType))
        {
            throw new AppException("The file MIME type is invalid.");
        }

        Song song = await _repo.SaveSongInS3(request.FileStream, request.FileName, request.ContentType, request.FileStream.Length, cancellationToken);
        song = await _repo.SaveSongMetadataInDb(song);

        return song.Id;
    }
}