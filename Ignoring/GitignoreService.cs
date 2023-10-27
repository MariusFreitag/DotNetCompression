using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetCompression.Compression;

namespace DotNetCompression.Ignoring
{
  public class GitignoreService : IIgnoreService
  {
    private readonly GlobIgnoreService generalIgnoreService = new();
    private readonly Dictionary<string, GlobIgnoreService> directoryIgnoreServices = new();

    public GitignoreService(DirectoryInfo gitDirectory)
    {
      generalIgnoreService.AddFilePatterns(".git/");

      foreach (FileInfo file in gitDirectory.GetFiles(".gitignore", SearchOption.AllDirectories))
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
      bool isIgnored = false;
      foreach ((string _, GlobIgnoreService ignoreService) in directoryIgnoreServices.Where(x => file.DirectoryName.Contains(x.Key)))
      {
        isIgnored = isIgnored || ignoreService.IsIgnored(file);
      }

      return isIgnored || generalIgnoreService.IsIgnored(file);
    }
  }
}
