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

    public event EventHandler<PurgationProgressEvent> Progress;

    public void Purge()
    {
      var files = options.Directory.GetFiles()
        .Where(x => DateTime.TryParseExact(x.Name, options.FileTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        .OrderBy(x => DateTime.ParseExact(x.Name, options.FileTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None));

      var healthyFiles = files.Where(x =>
      {
        try
        {
          ZipFile zipFile = new ZipFile(x.ToString());
          zipFile.Password = options.Password;
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
      }).ToArray();
      var corruptFiles = files.Where(x => !healthyFiles.Contains(x)).ToArray();
      var oldHealthyFiles = healthyFiles.SkipLast(options.KeepCount).ToArray();
      var residualFiles = healthyFiles.Where(x => !oldHealthyFiles.Contains(x)).ToArray();

      foreach (var file in residualFiles)
      {
        Progress?.Invoke(this, new PurgationProgressEvent
        {
          CurrentElement = file,
          Type = PurgationFileType.Residual,
          Exception = null
        });
      }

      foreach (var file in corruptFiles)
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

      foreach (var file in oldHealthyFiles)
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
