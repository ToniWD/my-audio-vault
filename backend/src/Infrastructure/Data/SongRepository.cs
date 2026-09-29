using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MusicStream.Application.Configuration;
using MusicStream.Application.DTOs;
using MusicStream.Application.Exceptions;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;
using MusicStream.Infrastructure.Utils;

namespace MusicStream.Infrastructure.Data;

public class SongRepository : ISongRepository
{
    private readonly AppDbContext _dbContext;
    private readonly IAmazonS3 _s3Client;
    private readonly string BucketName;

    public SongRepository(AppDbContext dbDbContext, IAmazonS3 client, IOptions<S3Settings> options)
    {
        _dbContext = dbDbContext;
        _s3Client = client;
        BucketName = options.Value.BucketName;
    }

    public async Task<PagedResponse<SongDetailsDto>> GetSongsDetails(SongQueryFilter filter, CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 50);

        var query = _dbContext.Songs.AsNoTracking().AsQueryable();

        // 1. Apply search filter (reduces the dataset)
        query = query.ApplySearch(filter.Search);

        // 2. Count total records AFTER filtering, BEFORE pagination
        var totalRecords = await query.CountAsync(cancellationToken);

        // 3. Apply sorting (default to Title if not specified)
        query = query.ApplySort(
            string.IsNullOrWhiteSpace(filter.SortBy) ? "Title" : filter.SortBy);

        // 4. Apply pagination and project to DTOs
        var songs = await query
            .ApplyPagination(pageNumber, pageSize)
            .Select(s => new SongDetailsDto(
                s.Id,
                s.Title,
                s.CoverImageUrl,
                s.Genre,
                s.ReleaseDate,
                s.IsExplicit,
                s.AudioFiles.Select(af => af.DurationSeconds).FirstOrDefault(),
                s.AudioFiles.Select(af => new AudioFileSummaryDto(
                    af.Id,
                    af.Format,
                    af.Quality,
                    af.BitrateKbps,
                    af.FileSizeBytes
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return new PagedResponse<SongDetailsDto>
        {
            Data = songs,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
    }

    public async Task<Song> SaveSongInS3(Stream fileStream, string fileName, string contentType, long fileSizeBytes, CancellationToken cancellationToken = default)
    {
        // Binary validation (Magic Bytes / Headers)
        if (!await AudioFileHelper.IsValidAudioHeaderAsync(fileStream))
        {
            throw new AppException("Binary signature check failed. The file is not a valid audio format.");
        }

        var key = $"audio/{Guid.NewGuid()}_{fileName}";
        var metadata = AudioFileHelper.ExtractSongMetadataFromAudioStream(fileStream, fileName, key, fileSizeBytes);

        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        var request = new PutObjectRequest
        {
            BucketName = BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType, // ex: "audio/mpeg", "audio/wav", "audio/ogg"
            AutoCloseStream = true
        };

        PutObjectResponse response = await _s3Client.PutObjectAsync(request, cancellationToken);

        return metadata;
    }

    public async Task<Song> SaveSongMetadataInDb(Song song, CancellationToken cancellationToken = default)
    {
        var entityEntry = await _dbContext.Songs.AddAsync(song, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return entityEntry.Entity;
    }
}