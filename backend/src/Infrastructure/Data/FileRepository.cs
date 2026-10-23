using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MusicStream.Application.Configuration;
using MusicStream.Application.DTOs;
using MusicStream.Application.Exceptions;
using MusicStream.Application.Interfaces;
using MusicStream.Core.Entities;
using MusicStream.Core.Enums;
using MusicStream.Infrastructure.Utils;

namespace MusicStream.Infrastructure.Data;

public class FileRepository : IFileRepository
{
    private readonly AppDbContext _dbContext;
    private readonly IAmazonS3 _s3Client;
    private readonly string BucketNameForAudioFiles;
    private readonly string BucketNameForImages;
    private readonly ILogger<FileRepository> _logger;

    public FileRepository(AppDbContext dbDbContext, IAmazonS3 client, IOptions<S3Settings> options, ILogger<FileRepository> logger)
    {
        _dbContext = dbDbContext;
        _s3Client = client;
        BucketNameForAudioFiles = options.Value.BucketNameAudioFiles;
        BucketNameForImages = options.Value.BucketNameImages;
        _logger = logger;
    }

    public PresignedUrl GetPresignedUrl(string key, StoredFileTypes type)
    {
        switch (type)
        {
            case StoredFileTypes.AUDIO_FILE:
                return GetPresignedUrl(key, BucketNameForAudioFiles, 2);
            case StoredFileTypes.COVER_IMAGE:
                return GetPresignedUrl(key, BucketNameForImages, 120);
            default:
                throw new AppException($"The file type {type} is not stored.");
        }
    }

    private PresignedUrl GetPresignedUrl(string key, string bucket, int minutes)
    {
        if (minutes <= 0)
        {
            throw new ArgumentException("Minutes must be > 0.");
        }
        try
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(minutes);
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = key,
                Expires = expiresAt,
                Protocol = Protocol.HTTP,
            };

            string url = _s3Client.GetPreSignedURL(request);

            return new PresignedUrl(url, new DateTimeOffset(expiresAt));
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred at GetPresignedUrlAsync for key {key} in the bucket {bucket}.\n {ex.Message}");
            throw new AppException("Something went wrong.");
        }
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
            BucketName = BucketNameForAudioFiles,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType, // ex: "audio/mpeg", "audio/wav", "audio/ogg"
            AutoCloseStream = true
        };

        PutObjectResponse response = await _s3Client.PutObjectAsync(request, cancellationToken);

        return metadata;
    }

    public async Task<string> SaveCoverImage(Stream fileStream, string fileName, string contentType, long fileSizeBytes, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var key = $"coverImage/{uniqueFileName}";

        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        var request = new PutObjectRequest
        {
            BucketName = BucketNameForImages,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
            AutoCloseStream = true
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        return key;
    }

    public async Task DeleteCoverImage(string key, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = BucketNameForImages,
            Key = key
        };
        await _s3Client.DeleteObjectAsync(request, cancellationToken);
    }
}