using MediatR;
using MusicStream.Application.DTOs;
using MusicStream.Application.Exceptions;
using MusicStream.Application.Interfaces;

namespace MusicStream.Application.UseCases;

public record GenerateImagePresignedUrlCommand(
    Guid songId
) : IRequest<PresignedUrl>;

public class GenerateImagePresignedUrlHandler : IRequestHandler<GenerateImagePresignedUrlCommand, PresignedUrl>
{
    private readonly IFileRepository FileRepo;
    private readonly ISongRepository SongRepo;

    public GenerateImagePresignedUrlHandler(IFileRepository fileRepository, ISongRepository songRepository)
    {
        FileRepo = fileRepository;
        SongRepo = songRepository;
    }

    public async Task<PresignedUrl> Handle(GenerateImagePresignedUrlCommand request, CancellationToken cancellationToken)
    {
        var song = await SongRepo.GetSongAsync(request.songId);
        if (song is null)
        {
            throw new NotFoundException($"No song found with the id: {request.songId}");
        }
        var presignedUrl = FileRepo.GetPresignedUrl(song.CoverImageKey, Core.Enums.StoredFileTypes.COVER_IMAGE);

        return presignedUrl;
    }
}