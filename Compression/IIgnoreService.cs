using System.IO;

namespace DotNetCompression.Compression
{
  public interface IIgnoreService
  {
    bool IsIgnored(FileInfo file);
  }
}
