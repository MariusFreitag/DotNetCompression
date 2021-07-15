using System.IO;
using System.Text.RegularExpressions;
using DotNetCompression.Compression;

namespace DotNetCompression
{
  public class GlobIgnoreService : IIgnoreService
  {
    private string ignorePattern = null;
    private Regex ignoreRegex = null;
    private string includePattern = null;
    private Regex includeRegex = null;

    public void AddFilePatterns(params string[] patterns)
    {
      foreach (var pattern in patterns)
      {
        var cleanedPattern = pattern;

        if (cleanedPattern.IndexOf("#") >= 0)
        {
          cleanedPattern = cleanedPattern.Substring(0, cleanedPattern.IndexOf("#"));
        }

        cleanedPattern = cleanedPattern.Replace('/', Path.DirectorySeparatorChar);
        cleanedPattern = cleanedPattern.Replace('\\', Path.DirectorySeparatorChar);
        cleanedPattern = cleanedPattern.Replace("|", @"\|");
        cleanedPattern = cleanedPattern.Replace("(", @"\(");
        cleanedPattern = cleanedPattern.Replace(")", @"\)");
        cleanedPattern = cleanedPattern.Replace(@"\", @"\\");
        cleanedPattern = cleanedPattern.Replace(".", @"\.");
        cleanedPattern = cleanedPattern.Replace("*", ".*");
        cleanedPattern = cleanedPattern.Replace("?", ".");
        cleanedPattern = cleanedPattern.Trim();

        var isNegated = cleanedPattern.StartsWith("!");
        if (isNegated)
        {
          cleanedPattern = cleanedPattern.Substring(1);
        }

        if (!cleanedPattern.StartsWith(Path.DirectorySeparatorChar))
        {
          cleanedPattern = Path.DirectorySeparatorChar + cleanedPattern;
        }
        if (!cleanedPattern.EndsWith(Path.DirectorySeparatorChar))
        {
          cleanedPattern = cleanedPattern + Path.DirectorySeparatorChar;
        }

        if (isNegated)
        {
          if (includePattern == null)
          {
            includePattern = $"({cleanedPattern})";
          }
          else
          {
            includePattern = $"{includePattern.ToString()}|({cleanedPattern})";
          }
          includePattern = new Regex(@"(\.)*(\.\*)+").Replace(includePattern, ".*");
          includeRegex = new Regex(includePattern);
        }
        else
        {
          if (ignorePattern == null)
          {
            ignorePattern = $"(.*{cleanedPattern})";
          }
          else
          {
            ignorePattern = $"{ignorePattern.ToString()}|({cleanedPattern})";
          }
          ignorePattern = new Regex(@"(\.)*(\.\*)+").Replace(ignorePattern, ".*");
          ignoreRegex = new Regex(ignorePattern);
        }
      }
    }

    public bool IsIgnored(FileInfo file)
    {
      var fileName = file.FullName;
      if (!fileName.StartsWith(Path.DirectorySeparatorChar))
      {
        fileName = Path.DirectorySeparatorChar + fileName;
      }
      if (!fileName.EndsWith(Path.DirectorySeparatorChar))
      {
        fileName = fileName + Path.DirectorySeparatorChar;
      }

      if (ignoreRegex == null)
      {
        return false;
      }

      return ignoreRegex.IsMatch(fileName) && (includeRegex == null || !includeRegex.IsMatch(fileName));
    }
  }
}
