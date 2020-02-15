using System;
using System.IO;

namespace DotNetCompression.Purgation
{
  public class PurgationProgressEvent
  {
    public FileSystemInfo CurrentElement { get; set; }
    public PurgationFileType Type { get; set; }
    public Exception Exception { get; set; }
  }
}
