using System.IO;
using System.Text.RegularExpressions;
using DotNetCompression.Compression;

namespace DotNetCompression.Ignoring
{
  public class GlobIgnoreService : IIgnoreService
  {
    private string ignorePattern;
    private Regex ignoreRegex;
    private string includePattern;
    private Regex includeRegex;

    public void AddFilePatterns(params string[] patterns)
    {
      foreach (string pattern in patterns)
      {
        string cleanedPattern = pattern;

        if (cleanedPattern.Contains('#', System.StringComparison.InvariantCulture))
        {
          cleanedPattern = cleanedPattern[..cleanedPattern.IndexOf('#')];
        }

        if (string.IsNullOrEmpty(cleanedPattern))
        {
          continue;
        }

        bool isNegated = cleanedPattern.StartsWith('!');
        if (isNegated)
        {
          cleanedPattern = cleanedPattern[1..];
        }

        if (!cleanedPattern.StartsWith(Path.DirectorySeparatorChar))
        {
          cleanedPattern = Path.DirectorySeparatorChar + cleanedPattern;
        }
        if (!cleanedPattern.EndsWith(Path.DirectorySeparatorChar))
        {
          cleanedPattern += Path.DirectorySeparatorChar;
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

        if (isNegated)
        {
          includePattern = includePattern == null ? $"({cleanedPattern})" : $"{includePattern}|({cleanedPattern})";
          includePattern = new Regex(@"(\.)*(\.\*)+").Replace(includePattern, ".*");
          includeRegex = new Regex(includePattern);
        }
        else
        {
          ignorePattern = ignorePattern == null ? $"(.*{cleanedPattern})" : $"{ignorePattern}|({cleanedPattern})";
          ignorePattern = new Regex(@"(\.)*(\.\*)+").Replace(ignorePattern, ".*");
          ignoreRegex = new Regex(ignorePattern);
        }
      }
    }

    public bool IsIgnored(FileInfo file)
    {
      string fileName = file.FullName;
      if (!fileName.StartsWith(Path.DirectorySeparatorChar))
      {
        fileName = Path.DirectorySeparatorChar + fileName;
      }
      if (!fileName.EndsWith(Path.DirectorySeparatorChar))
      {
        fileName += Path.DirectorySeparatorChar;
      }

      return ignoreRegex != null && ignoreRegex.IsMatch(fileName) && (includeRegex == null || !includeRegex.IsMatch(fileName));
    }
  }
}
