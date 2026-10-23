using MusicStream.Application.DTOs;
using MusicStream.Core.Entities;

namespace MusicStream.Application.Interfaces;

public interface ISongRepository
{
    Task<PagedResponse<SongDetailsDto>> GetSongsDetails(SongQueryFilter filter, CancellationToken cancellationToken = default);
    Task<SongDetailsDto> GetSongDetailsById(Guid songId, CancellationToken cancellationToken = default);
    Task<Song> SaveSongMetadataInDb(Song song, CancellationToken cancellationToken = default);
    Task<Song?> GetSongAsync(Guid songId);
    Task<Song?> GetSongWithAudioFilesAsync(Guid songId);
    Task UpdateSongCoverAsync(Guid songId, string newCoverKey, CancellationToken cancellationToken);
}