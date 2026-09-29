using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using MusicStream.Infrastructure.Data;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MusicStream.Application.Configuration;
using MusicStream.Core.Entities;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Logging;

namespace MusicStream.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18")
    .WithDatabase("musicStream_test_db")
    .WithUsername("postgres_tests")
    .WithPassword("integration_tests_db")
    .Build();

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        using var dbContext = new AppDbContext(options);
        await dbContext.Database.MigrateAsync();
        await SeedDatabase(dbContext);
    }

    private async Task SeedDatabase(AppDbContext dbContext)
    {
        // Verificăm dacă baza de date conține deja date pentru a evita duplicarea
        if (await dbContext.Songs.AnyAsync())
        {
            return;
        }

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<CustomWebApplicationFactory>();

        var genres = new[] { "Pop", "Rock", "Hip-Hop", "Electronic", "Jazz", "Indie" };

        var songs = new List<Song>();

        for (int i = 1; i <= 25; i++)
        {
            var songId = Guid.NewGuid();
            var genre = genres[(i - 1) % genres.Length];
            var duration = 180 + (i * 7 % 120); // Durate între 3 și 5 minute

            var song = new Song
            {
                Title = $"Track Title #{i:00}",
                CoverImageUrl = $"https://picsum.photos/seed/song{i}/300/300",
                Genre = genre,
                ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i * 15)),
                IsExplicit = i % 4 == 0,
                AudioFiles = new List<AudioFile>
            {
                new AudioFile
                {
                    Id = Guid.NewGuid(),
                    SongId = songId,
                    AudioUrl = $"https://cdn.example.com/audio/song_{i}_med.mp3",
                    Format = "mp3",
                    Quality = "Medium",
                    DurationSeconds = duration,
                    BitrateKbps = 256,
                    SampleRateHz = 44100,
                    Channels = 2,
                    FileSizeBytes = duration * 256 * 1024 / 8,
                    Checksum = $"chk_med_{i}"
                },
                new AudioFile
                {
                    Id = Guid.NewGuid(),
                    SongId = songId,
                    AudioUrl = $"https://cdn.example.com/audio/song_{i}_high.flac",
                    Format = "flac",
                    Quality = "Lossless",
                    DurationSeconds = duration,
                    BitrateKbps = 1411,
                    SampleRateHz = 44100,
                    Channels = 2,
                    FileSizeBytes = duration * 1411 * 1024 / 8,
                    Checksum = $"chk_high_{i}"
                }
            }
            };

            songs.Add(song);
        }

        await dbContext.Songs.AddRangeAsync(songs);
        await dbContext.SaveChangesAsync();

        var nrSongs = await dbContext.Songs.CountAsync();
        var nrAudioFiles = await dbContext.AudioFiles.CountAsync();

        logger.LogInformation("Database seeded successfully with {SongsCount} songs and {AudioFilesCount} audio files.", nrSongs, nrAudioFiles);
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove original configuration for AppDbContext from program
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            // Add a new AppDbContext
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            var testAuthSettings = new AuthenticationSettings
            {
                TokenSecret = $"MUSIC_STREAM_TEST_TOKEN_SECRET_KEY_32_BYTES_LONG",
                RefreshTokenSecret = $"MUSIC_STREAM_TEST_REFRESH_TOKEN_SECRET_KEY_32_BYTES_LONG",
                Issuer = "MusicStreamTestIssuer",
                Audience = "MusicStreamTestAudience"
            };

            services.RemoveAll(typeof(IOptions<AuthenticationSettings>));
            services.AddSingleton<IOptions<AuthenticationSettings>>(Options.Create(testAuthSettings));
        });

        base.ConfigureWebHost(builder);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}