namespace MusicStream.Core.Domain.Entities;

public class Playlist
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "Untitled";
    public string? ThumbnailUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Song> Songs { get; set; } = new List<Song>();

    public Playlist() { }

    public Playlist(string title, string? thumbnailUrl = null)
    {
        Title = title;
        ThumbnailUrl = thumbnailUrl;
    }
}