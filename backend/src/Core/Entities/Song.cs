namespace MusicStream.Core.Entities;

public class Song
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public required string Title { get; set; }
    public required string CoverImageUrl { get; set; }
    public List<Guid> ArtistIds { get; set; } = [];
    public Guid? AlbumId { get; set; }

    public required string Genre { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public bool IsExplicit { get; set; }

    public long PlayCount { get; private set; }
    public int LikeCount { get; private set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    public ICollection<AudioFile> AudioFiles { get; set; } = new List<AudioFile>();

    public void IncrementPlayCount()
    {
        PlayCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateLikes(int delta)
    {
        LikeCount = Math.Max(0, LikeCount + delta);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}