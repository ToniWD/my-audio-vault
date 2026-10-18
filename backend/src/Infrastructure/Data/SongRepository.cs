using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicStream.Application.Configuration;
using MusicStream.Application.DTOs;
using MusicStream.Application.Exceptions;
using MusicStream.Application.Interfaces;
using MusicStream.Application.UseCases;
using MusicStream.Core.Entities;
using MusicStream.Infrastructure.Utils;

namespace MusicStream.Infrastructure.Data;

public class SongRepository : ISongRepository
{
    private readonly AppDbContext _dbContext;
    private readonly IAmazonS3 _s3Client;
    private readonly string BucketName;
    private readonly ILogger<SongRepository> _logger;

    public SongRepository(AppDbContext dbDbContext, IAmazonS3 client, IOptions<S3Settings> options, ILogger<SongRepository> logger)
    {
        _dbContext = dbDbContext;
        _s3Client = client;
        BucketName = options.Value.BucketName;
        _logger = logger;
    }

    public async Task<AudioStream?> GetAudioStreamAsync(AudioFile audioFile, string? range = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = BucketName,
                Key = audioFile.AudioUrl
            };

            if (!string.IsNullOrEmpty(range))
            {
                request.ByteRange = new ByteRange(range);
            }

            var response = await _s3Client.GetObjectAsync(request, cancellationToken);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK &&
                response.HttpStatusCode != System.Net.HttpStatusCode.PartialContent)
            {
                _logger.LogError($"S3 error. Something went wrong with the request: {request.ToString()}");
                return null;
            }

            var contentType = string.IsNullOrEmpty(response.Headers.ContentType) ? "audio/mpeg" : response.Headers.ContentType;

            return new AudioStream(
                response.ResponseStream,
                contentType,
                response.ContentLength,
                response.ContentRange);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError($"S3 error. Something went wrong. \n{ex.StackTrace}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong. \n{ex.StackTrace}");
            return null;
        }
    }

    public async Task<Song?> GetSongAsync(Guid songId)
    {
        return await _dbContext.Songs.FindAsync(songId);
    }

    public async Task<SongDetailsDto?> GetSongDetailsById(Guid songId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Songs.AsNoTracking().AsQueryable();

        query = query.Where(s => s.Id == songId);

        var song = await query
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
            .FirstOrDefaultAsync(cancellationToken);

        return song;
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

    public async Task<Song?> GetSongWithAudioFilesAsync(Guid songId)
    {
        return await _dbContext.Songs
         .Include(s => s.AudioFiles)
         .FirstOrDefaultAsync(s => s.Id == songId);
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