namespace MusicStream.Core.Domain.Entities;

public class AudioFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FilePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string ContentType { get; set; } = "audio/mpeg";
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public AudioFile() { }

    public AudioFile(string filePath, string originalFileName, TimeSpan duration, string contentType, long fileSizeBytes)
    {
        FilePath = filePath;
        OriginalFileName = originalFileName;
        Duration = duration;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
    }
}