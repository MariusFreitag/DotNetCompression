using System.Collections.Generic;
using System.IO;
using DotNetCompression.Compression;

namespace DotNetCompression.Ignoring
{
  public class CombinedIgnoreService : IIgnoreService
  {
    private readonly List<IIgnoreService> ignoreServices = new();

    public bool IsIgnored(FileInfo file)
    {
      foreach (IIgnoreService ignoreService in ignoreServices)
      {
        if (ignoreService.IsIgnored(file))
        {
          return true;
        }
      }
      return false;
    }

    public void AddIgnoreService(IIgnoreService ignoreService)
    {
      ignoreServices.Add(ignoreService);
    }
  }
}
