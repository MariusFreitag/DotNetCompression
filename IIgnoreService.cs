using System.IO;

namespace Compression
{
  public interface IIgnoreService
  {
    bool IsIgnored(FileInfo file);
  }
}
