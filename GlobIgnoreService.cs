using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Compression
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

        if (cleanedPattern.Length > 0)
        {
          if (cleanedPattern.StartsWith("!"))
          {
            cleanedPattern = cleanedPattern.Substring(1);

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
    }

    public bool IsIgnored(FileInfo file)
    {
      if (ignoreRegex == null)
      {
        return false;
      }

      if (includeRegex == null)
      {
        return ignoreRegex.IsMatch(file.FullName);
      }

      return ignoreRegex.IsMatch(file.FullName) && !includeRegex.IsMatch(file.FullName);
    }
  }
}
