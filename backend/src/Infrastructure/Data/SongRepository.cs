using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicStream.Application.Configuration;
using MusicStream.Application.DTOs;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;

namespace MusicStream.Infrastructure.Data;

public class SongRepository : ISongRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SongRepository> _logger;

    public SongRepository(AppDbContext dbDbContext, IOptions<S3Settings> options, ILogger<SongRepository> logger)
    {
        _dbContext = dbDbContext;
        _logger = logger;
    }

    public async Task<Song?> GetSongAsync(Guid songId)
    {
        return await _dbContext.Songs.FindAsync(songId);
    }

    public async Task<SongDetailsDto> GetSongDetailsById(Guid songId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Songs.AsNoTracking().AsQueryable();

        query = query.Where(s => s.Id == songId);

        var song = await query
            .Select(s => new SongDetailsDto(
                s.Id,
                s.Title,
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

    public async Task<Song> SaveSongMetadataInDb(Song song, CancellationToken cancellationToken = default)
    {
        var entityEntry = await _dbContext.Songs.AddAsync(song, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return entityEntry.Entity;
    }

    public async Task UpdateSongCoverAsync(Guid songId, string newCoverKey, CancellationToken cancellationToken)
    {
        await _dbContext.Songs
            .Where(s => s.Id == songId)
            .ExecuteUpdateAsync(s => s.SetProperty(song => song.CoverImageKey, newCoverKey), cancellationToken);
    }
}