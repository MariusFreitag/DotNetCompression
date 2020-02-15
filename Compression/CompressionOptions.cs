using System.IO;

namespace DotNetCompression.Compression
{
  public class CompressionOptions
  {
    public DirectoryInfo Source { get; set; }
    public FileInfo Destination { get; set; }
    public bool OverrideDestination { get; set; }
    public CompressionLevel CompressionLevel { get; set; }
    public string Password { get; set; }
  }
}
