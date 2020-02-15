using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetCompression.Compression;

namespace DotNetCompression
{
  public class GitignoreService : IIgnoreService
  {
    private GlobIgnoreService generalIgnoreService = new GlobIgnoreService();
    private Dictionary<string, GlobIgnoreService> directoryIgnoreServices = new Dictionary<string, GlobIgnoreService>();

    public GitignoreService(DirectoryInfo gitDirectory)
    {
      generalIgnoreService.AddFilePatterns(".git/");

      foreach (var file in gitDirectory.GetFiles(".gitignore", SearchOption.AllDirectories))
      {
        if (!directoryIgnoreServices.ContainsKey(file.DirectoryName))
        {
          directoryIgnoreServices[file.DirectoryName] = new GlobIgnoreService();
        }
        directoryIgnoreServices[file.DirectoryName].AddFilePatterns(File.ReadAllLines(file.FullName));
      }
    }

    public bool IsIgnored(FileInfo file)
    {
      var isIgnored = false;
      foreach (var (_, ignoreService) in directoryIgnoreServices.Where(x => file.DirectoryName.Contains(x.Key)))
      {
        isIgnored = isIgnored || ignoreService.IsIgnored(file);
      }

      return isIgnored || generalIgnoreService.IsIgnored(file);
    }
  }
}
