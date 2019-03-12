using System.IO;

namespace Compression
{
  public class CompressionProgressEvent
  {
    public int TotalCount { get; set; }
    public int CurrentCount { get; set; }
    public FileSystemInfo CurrentElement { get; set; }
    public bool Ignored { get; set; }
  }
}
