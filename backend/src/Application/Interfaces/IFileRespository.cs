using MusicStream.Application.DTOs;
using MusicStream.Core.Entities;
using MusicStream.Core.Enums;

namespace MusicStream.Application.Interfaces;

public interface IFileRepository
{
    Task<Song> SaveSongInS3(Stream FileStream, string FileName, string ContentType, long fileSizeBytes, CancellationToken cancellationToken = default);
    Task<string> SaveCoverImage(Stream FileStream, string FileName, string ContentType, long fileSizeBytes, CancellationToken cancellationToken = default);
    PresignedUrl GetPresignedUrl(string key, StoredFileTypes type);
    Task DeleteCoverImage(string key, CancellationToken cancellationToken = default);
}