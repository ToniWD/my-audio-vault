using System.Security.Cryptography.X509Certificates;

namespace MusicStream.Application.Configuration;

public record S3Settings
{
    public required string AccessKey { get; init; }
    public required string SecretKey { get; init; }
    public required string BucketName { get; init; }
    public required string Region { get; init; }
    public required string ServiceUrl { get; init; }
    public required bool ForcePathStyle { get; init; }
};