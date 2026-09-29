using MusicStream.Application.DTOs;
using MusicStream.Core.Entities;

namespace MusicStream.Application.Interfaces;

public interface ISongRepository
{
    Task<PagedResponse<SongDetailsDto>> GetSongsDetails(SongQueryFilter filter, CancellationToken cancellationToken = default);
    Task<Song> SaveSongInS3(Stream FileStream, string FileName, string ContentType, long fileSizeBytes, CancellationToken cancellationToken = default);
    Task<Song> SaveSongMetadataInDb(Song song, CancellationToken cancellationToken = default);
}