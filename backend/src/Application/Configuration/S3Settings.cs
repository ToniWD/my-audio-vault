namespace MusicStream.Application.Configuration;

public record S3Settings
{
    public required string AccessKey { get; init; }
    public required string SecretKey { get; init; }
    public required string BucketNameAudioFiles { get; init; }
    public required string BucketNameImages { get; init; }
    public required string Region { get; init; }
    public required string ServiceUrl { get; init; }
    public required bool ForcePathStyle { get; init; }
};