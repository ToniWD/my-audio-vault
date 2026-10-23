using MediatR;
using MusicStream.Application.DTOs;
using MusicStream.Application.Exceptions;
using MusicStream.Application.Interfaces;

namespace MusicStream.Application.UseCases;

public record GenerateSongPresignedUrlCommand(
    Guid SongId
) : IRequest<PresignedUrl>;

public class GenerateAudioFilePresignedUrlHandler : IRequestHandler<GenerateSongPresignedUrlCommand, PresignedUrl>
{
    private readonly ISongRepository SongRepo;
    private readonly IFileRepository FileRepo;

    public GenerateAudioFilePresignedUrlHandler(ISongRepository songRepository, IFileRepository fileRepository)
    {
        SongRepo = songRepository;
        FileRepo = fileRepository;
    }

    public async Task<PresignedUrl> Handle(GenerateSongPresignedUrlCommand request, CancellationToken cancellationToken)
    {
        var song = await SongRepo.GetSongWithAudioFilesAsync(request.SongId);
        if (song == null) throw new NotFoundException($"No song found with the id: {request.SongId}");

        var audioFile = song.AudioFiles.FirstOrDefault();

        if (audioFile is null) throw new NotFoundException($"No audio file stored for the song with the id: {request.SongId}");

        var presignedUrl = FileRepo.GetPresignedUrl(audioFile.AudioUrl, Core.Enums.StoredFileTypes.AUDIO_FILE);

        return presignedUrl;
    }
}