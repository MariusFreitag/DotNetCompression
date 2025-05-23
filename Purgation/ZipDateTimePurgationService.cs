using System;
using System.Globalization;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace DotNetCompression.Purgation
{
  public class ZipDateTimePurgationService(PurgationOptions options) : IPurgationService
  {
    private readonly PurgationOptions options = options;

    public event EventHandler<PurgationProgressEvent> Progress;

    public void Purge()
    {
      IOrderedEnumerable<System.IO.FileInfo> files = options.Directory.GetFiles()
        .Where(x => DateTime.TryParseExact(x.Name, options.FileTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        .OrderBy(x => DateTime.ParseExact(x.Name, options.FileTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None));

      System.IO.FileInfo[] healthyFiles = [.. files.Where(x =>
      {
        try
        {
          ZipFile zipFile = new(x.ToString())
          {
            Password = options.Password
          };
          bool isHealthy = zipFile.TestArchive(true, TestStrategy.FindAllErrors, delegate (TestStatus status, string message)
          {
            if (!string.IsNullOrWhiteSpace(message))
            {
              Console.WriteLine(message);
            }
          });
          zipFile.Close();
          return isHealthy;
        }
        catch
        {
          return false;
        }
      })];
      System.IO.FileInfo[] corruptFiles = [.. files.Where(x => !healthyFiles.Contains(x))];
      System.IO.FileInfo[] oldHealthyFiles = [.. healthyFiles.SkipLast(options.KeepCount)];
      System.IO.FileInfo[] residualFiles = [.. healthyFiles.Where(x => !oldHealthyFiles.Contains(x))];

      foreach (System.IO.FileInfo file in residualFiles)
      {
        Progress?.Invoke(this, new PurgationProgressEvent
        {
          CurrentElement = file,
          Type = PurgationFileType.Residual,
          Exception = null
        });
      }

      foreach (System.IO.FileInfo file in corruptFiles)
      {
        Exception exception = null;
        try
        {
          file.Delete();
        }
        catch (Exception e)
        {
          exception = e;
        }

        Progress?.Invoke(this, new PurgationProgressEvent
        {
          CurrentElement = file,
          Type = PurgationFileType.Corrupt,
          Exception = exception
        });
      }

      foreach (System.IO.FileInfo file in oldHealthyFiles)
      {
        Exception exception = null;
        try
        {
          file.Delete();
        }
        catch (Exception e)
        {
          exception = e;
        }

        Progress?.Invoke(this, new PurgationProgressEvent
        {
          CurrentElement = file,
          Type = PurgationFileType.Old,
          Exception = exception
        });
      }
    }
  }
}
