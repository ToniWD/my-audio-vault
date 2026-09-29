using MusicStream.Core.Entities;

namespace MusicStream.Infrastructure.Utils;

public static class AudioFileHelper
{
    /// <summary>
    /// Extracts metadata from an audio file stream and constructs a <see cref="Song"/> entity with an associated <see cref="AudioFile"/>.
    /// </summary>
    /// <param name="stream">The readable audio stream. The stream position will be reset to 0 if seekable.</param>
    /// <param name="fileName">The original file name. Used as a fallback title (without extension) if metadata does not contain a title.</param>
    /// <param name="storageObjectKey">The storage reference/key (e.g., S3 object key) for the audio file.</param>
    /// <param name="fileSizeBytes">The total size of the file in bytes.</param>
    /// <returns>A new <see cref="Song"/> entity containing the extracted metadata and the audio file payload.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/>, <paramref name="fileName"/>, or <paramref name="storageObjectKey"/> is null.</exception>
    public static Song ExtractSongMetadataFromAudioStream(
        Stream stream,
        string fileName,
        string storageObjectKey,
        long fileSizeBytes)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(storageObjectKey);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        var abstraction = new StreamFileAbstraction(fileName, stream);
        using var tagFile = TagLib.File.Create(abstraction);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        var tag = tagFile.Tag;
        var properties = tagFile.Properties;

        string songTitle = tag != null && !string.IsNullOrWhiteSpace(tag.Title)
            ? tag.Title
            : Path.GetFileNameWithoutExtension(fileName);

        string genre = tag != null && !string.IsNullOrWhiteSpace(tag.FirstGenre)
            ? tag.FirstGenre
            : "Unknown";

        DateOnly releaseDate = (tag != null && tag.Year > 0)
            ? new DateOnly((int)tag.Year, 1, 1)
            : DateOnly.FromDateTime(DateTime.UtcNow);

        var song = new Song
        {
            Title = songTitle,
            Genre = genre,
            ReleaseDate = releaseDate,
            CoverImageUrl = string.Empty,
            IsExplicit = false
        };

        int bitrate = properties?.AudioBitrate ?? 0;
        int sampleRate = properties?.AudioSampleRate ?? 0;
        short channels = (short)(properties?.AudioChannels ?? 0);
        int durationSeconds = properties != null ? (int)Math.Round(properties.Duration.TotalSeconds) : 0;

        var audioFile = new AudioFile
        {
            Id = Guid.NewGuid(),
            SongId = song.Id,
            AudioUrl = storageObjectKey,
            Format = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant(),
            Quality = $"{bitrate} kbps",
            DurationSeconds = durationSeconds,
            BitrateKbps = bitrate,
            SampleRateHz = sampleRate,
            Channels = channels,
            FileSizeBytes = fileSizeBytes
        };

        song.AudioFiles.Add(audioFile);

        return song;
    }

    /// <summary>
    /// Checks if the audio stream contains a valid magic number/header for common audio file formats (MP3, WAV, OGG, FLAC, M4A/AAC).
    /// </summary>
    /// <param name="stream">The readable stream. The stream position will be reset to 0 before and after reading if seekable.</param>
    /// <returns><see langword="true"/> if the stream starts with a recognized audio header; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="stream"/> is not readable.</exception>
    public static async Task<bool> IsValidAudioHeaderAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            throw new ArgumentException("The stream must be readable.", nameof(stream));
        }

        long originalPosition = 0;
        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
            stream.Position = 0;
        }

        try
        {
            var headerBytes = new byte[12];

            int bytesRead = await ReadAtMostAsync(stream, headerBytes, cancellationToken);

            if (bytesRead < 4) return false;

            // MP3 without ID3v2 (Frame Header: 0xFF 0xE0..0xFF)
            bool isStandardMp3 = headerBytes[0] == 0xFF && (headerBytes[1] & 0xE0) == 0xE0;

            // MP3 with ID3v2 container ("ID3")
            bool isId3Mp3 = headerBytes[0] == 0x49 && headerBytes[1] == 0x44 && headerBytes[2] == 0x33;

            // WAV ("RIFF")
            bool isWav = headerBytes[0] == 0x52 && headerBytes[1] == 0x49 && headerBytes[2] == 0x46 && headerBytes[3] == 0x46;

            // OGG ("OggS")
            bool isOgg = headerBytes[0] == 0x4F && headerBytes[1] == 0x67 && headerBytes[2] == 0x67 && headerBytes[3] == 0x53;

            // FLAC ("fLaC")
            bool isFlac = headerBytes[0] == 0x66 && headerBytes[1] == 0x4C && headerBytes[2] == 0x61 && headerBytes[3] == 0x43;

            // M4A / MP4 / AAC
            bool isM4a = bytesRead >= 8 &&
                         headerBytes[4] == 0x66 && // 'f'
                         headerBytes[5] == 0x74 && // 't'
                         headerBytes[6] == 0x79 && // 'y'
                         headerBytes[7] == 0x70;   // 'p'

            return isStandardMp3 || isId3Mp3 || isWav || isOgg || isFlac || isM4a;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }

    private static async Task<int> ReadAtMostAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        int totalBytesRead = 0;
        while (totalBytesRead < buffer.Length)
        {
            int bytesRead = await stream.ReadAsync(
                buffer.AsMemory(totalBytesRead, buffer.Length - totalBytesRead),
                cancellationToken);

            if (bytesRead == 0) break; // End of stream
            totalBytesRead += bytesRead;
        }
        return totalBytesRead;
    }
}