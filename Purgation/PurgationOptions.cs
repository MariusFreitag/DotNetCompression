using System.IO;

namespace DotNetCompression.Purgation
{
  public class PurgationOptions
  {
    public DirectoryInfo Directory { get; set; }
    public string FileTimeFormat { get; set; }
    public int KeepCount { get; set; }
    public bool PurgeCorruptFiles { get; set; }
    public string Password { get; set; }
  }
}
