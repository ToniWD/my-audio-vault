namespace MusicStream.Infrastructure.Utils;

public class StreamFileAbstraction : TagLib.File.IFileAbstraction
{
    public string Name { get; }
    public Stream ReadStream { get; }
    public Stream WriteStream => throw new NotSupportedException();

    public StreamFileAbstraction(string name, Stream stream)
    {
        Name = name;
        ReadStream = stream;
    }

    public void CloseStream(Stream stream)
    {
        // Don't close the stream, to make sure we can used it after
    }
}