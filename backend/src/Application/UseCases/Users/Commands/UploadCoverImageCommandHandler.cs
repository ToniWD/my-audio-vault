
using MediatR;
using Microsoft.Extensions.Logging;
using MusicStream.Application.DTOs;
using MusicStream.Application.Exceptions;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;

namespace MusicStream.Application.UseCases;

public record UploadCoverImageCommand(
    Guid SongId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long Length
) : IRequest<PresignedUrl>;

public class UploadCoverImageCommandHandler : IRequestHandler<UploadCoverImageCommand, PresignedUrl>
{
    private readonly ISongRepository SongRepo;
    private readonly IFileRepository FileRepo;
    private readonly ILogger<UploadCoverImageCommandHandler> _logger;

    public UploadCoverImageCommandHandler(ISongRepository repository, IFileRepository fileRepository, ILogger<UploadCoverImageCommandHandler> logger)
    {
        SongRepo = repository;
        FileRepo = fileRepository;
        _logger = logger;
    }

    public async Task<PresignedUrl> Handle(UploadCoverImageCommand request, CancellationToken cancellationToken)
    {
        const long maxFileSize = 5 * 1024 * 1024; // 5 MB
        if (request.FileStream.Length > maxFileSize)
        {
            throw new AppException("File size exceeds the maximum allowed limit of 5 MB.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            throw new AppException("The provided file extension is not supported.");
        }

        if (!allowedMimeTypes.Contains(request.ContentType))
        {
            throw new AppException("The file MIME type is invalid.");
        }

        Song? song = await SongRepo.GetSongAsync(request.SongId);
        if (song is null)
        {
            throw new NotFoundException($"No song found with the id: {request.SongId}");
        }

        string key = await FileRepo.SaveCoverImage(request.FileStream, request.FileName, request.ContentType, request.Length, cancellationToken);

        if (!string.IsNullOrWhiteSpace(song.CoverImageKey))
        {
            await FileRepo.DeleteCoverImage(song.CoverImageKey, cancellationToken);
        }

        await SongRepo.UpdateSongCoverAsync(song.Id, key, cancellationToken);

        var presignedUrl = FileRepo.GetPresignedUrl(key, Core.Enums.StoredFileTypes.COVER_IMAGE);

        return presignedUrl;
    }
}