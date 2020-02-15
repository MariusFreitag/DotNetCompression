using System;
using System.Globalization;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace DotNetCompression.Purgation
{
  public class ZipDateTimePurgationService : IPurgationService
  {
    private readonly PurgationOptions options;

    public ZipDateTimePurgationService(PurgationOptions options)
    {
      this.options = options;
    }
    public void Purge()
    {
      var files = options.Directory.GetFiles()
        .Where(x => DateTime.TryParseExact(x.Name, options.FileTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        .OrderBy(x => DateTime.ParseExact(x.Name, options.FileTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None));

      var healthyFiles = files.Where(x =>
      {
        try
        {
          var zipFile = new ZipFile(x.ToString());
          var isHealthy = zipFile.TestArchive(true);
          zipFile.Close();
          return isHealthy;
        }
        catch
        {
          return false;
        }
      }).ToArray();
      var corruptFiles = files.Where(x => !healthyFiles.Contains(x)).ToArray();
      var oldHealthyFiles = healthyFiles.SkipLast(options.KeepCount).ToArray();
      var residualFiles = healthyFiles.Where(x => !oldHealthyFiles.Contains(x)).ToArray();

      foreach (var file in residualFiles)
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Keeping File: {file.FullName}");
      }

      foreach (var file in corruptFiles)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Deleting corrupt file: {file.FullName}");

        try
        {
          file.Delete();
        }
        catch (Exception e)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Deletetion failed: {e.Message}");
        }
      }

      foreach (var file in oldHealthyFiles)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Deleting old file: {file.FullName}");

        try
        {
          file.Delete();
        }
        catch (Exception e)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Deletetion failed: {e.Message}");
        }
      }

      Console.ForegroundColor = ConsoleColor.Gray;
    }
  }
}
