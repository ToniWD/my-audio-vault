using MediatR;
using MusicStream.Application.Exceptions;
using MusicStream.Application.Interfaces;

namespace MusicStream.Application.UseCases;

public record GetAudioStreamQuery(
    Guid SongId,
    string? ByteRange = null
) : IRequest<AudioStream>;

public record AudioStream(
    Stream Stream,
    string ContentType,
    long ContentLength,
    // Set by the storage layer only when it honoured a byte-range request,
    // e.g. "bytes 500-999/1000". Null for a full-object response.
    string? ContentRange
);

public class GetAudioStreamHandler : IRequestHandler<GetAudioStreamQuery, AudioStream>
{
    private readonly ISongRepository _repo;

    public GetAudioStreamHandler(ISongRepository repository)
    {
        _repo = repository;
    }
    public async Task<AudioStream> Handle(GetAudioStreamQuery request, CancellationToken cancellationToken)
    {
        var song = await _repo.GetSongWithAudioFilesAsync(request.SongId);
        if (song == null) throw new NotFoundException($"No song found with the id: {request.SongId}");

        var audioFile = song.AudioFiles.FirstOrDefault();

        if (audioFile is null) throw new NotFoundException($"No audio file stored for the song with the id: {request.SongId}");

        var audioStream = await _repo.GetAudioStreamAsync(audioFile, request.ByteRange, cancellationToken);

        if (audioStream == null) throw new NotFoundException($"No song found with the id: {request.SongId}");

        return audioStream;
    }
}