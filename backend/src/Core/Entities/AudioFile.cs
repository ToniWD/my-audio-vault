namespace MusicStream.Core.Entities;

public class AudioFile
{
    public required Guid Id { get; init; }
    public required Guid SongId { get; init; }

    public required string AudioUrl { get; set; }
    public required string Format { get; init; }
    public required string Quality { get; init; }

    public required int DurationSeconds { get; init; }
    public int BitrateKbps { get; init; }
    public int SampleRateHz { get; init; }
    public short Channels { get; init; } = 2;
    public long FileSizeBytes { get; init; }

    public string? Checksum { get; init; }

    public IReadOnlyList<float> WaveformData { get; init; } = [];

    public Song Song { get; init; } = null!;
}