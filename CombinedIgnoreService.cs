using System.Collections.Generic;
using System.IO;
using DotNetCompression.Compression;

namespace DotNetCompression
{
  public class CombinedIgnoreService : IIgnoreService
  {
    private List<IIgnoreService> ignoreServices = new List<IIgnoreService>();

    public bool IsIgnored(FileInfo file)
    {
      foreach (var ignoreService in this.ignoreServices)
      {
        if (ignoreService.IsIgnored(file))
        {
          return true;
        }
      }
      return false;
    }

    public void addIgnoreService(IIgnoreService ignoreService)
    {
      this.ignoreServices.Add(ignoreService);
    }
  }
}
