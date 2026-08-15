namespace MusicStream.Core.Entities;

public class Song
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? ArtistName { get; set; }
    public string? AlbumName { get; set; }
    public string? Genre { get; set; }

    public long PlayCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid AudioFileId { get; set; }
    public AudioFile AudioFile { get; set; } = null!;

    public Song() { }

    public Song(string title, Guid audioFileId, string? artistName = null, string? thumbnailUrl = null)
    {
        Title = title;
        AudioFileId = audioFileId;
        ArtistName = artistName;
        ThumbnailUrl = thumbnailUrl;
    }
}
