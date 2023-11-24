using System;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace DotNetCompression.Compression
{
  public class ZipCompressionService(IIgnoreService ignoreService, CompressionOptions options) : ICompressionService
  {
    private readonly IIgnoreService ignoreService = ignoreService;
    private readonly CompressionOptions options = options;

    private int folderOffset;
    private ZipOutputStream zipOutputStream;

    private int totalFileCount;
    private int currentFileCount;

    public event EventHandler<CompressionProgressEvent> Progress;

    public async Task CompressAsync()
    {
      try
      {
        options.Destination.Directory.Create();

        if (options.Destination.Exists)
        {
          if (options.OverrideDestination)
          {
            options.Destination.Delete();
          }
          else
          {
            throw new CompressionException($"Destination file '{options.Destination.FullName}' does already exist");
          }
        }

        totalFileCount = options.Source.GetFiles("*", SearchOption.AllDirectories).Length;

        zipOutputStream = new ZipOutputStream(options.Destination.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite));
        zipOutputStream.SetLevel((int)options.CompressionLevel);
        zipOutputStream.Password = options.Password;

        folderOffset = options.Source.FullName.Length + (options.Source.FullName.EndsWith('\\') ? 0 : 1);

        await CompressFolderAsync(options.Source);

        zipOutputStream.IsStreamOwner = true;
        zipOutputStream.Close();
      }
      catch (CompressionException)
      {
        throw;
      }
      catch (Exception e)
      {
        throw new CompressionException(e.Message, e);
      }
    }

    private async Task CompressFolderAsync(DirectoryInfo directory)
    {
      foreach (FileInfo file in directory.GetFiles())
      {
        currentFileCount++;
        bool isIgnored = ignoreService.IsIgnored(file);

        Progress?.Invoke(this, new CompressionProgressEvent
        {
          TotalCount = totalFileCount,
          CurrentCount = currentFileCount,
          CurrentElement = file,
          Ignored = isIgnored
        });

        if (isIgnored)
        {
          continue;
        }

        string entryName = file.FullName[folderOffset..];
        entryName = ZipEntry.CleanName(entryName);
        ZipEntry newEntry = new(entryName)
        {
          DateTime = file.LastWriteTime,

          Size = file.Length
        };

        zipOutputStream.PutNextEntry(newEntry);

        using (FileStream streamReader = file.OpenRead())
        {
          StreamUtils.Copy(streamReader, zipOutputStream, new byte[4096]);
        }

        zipOutputStream.CloseEntry();
      }

      foreach (DirectoryInfo subdirectory in directory.GetDirectories())
      {
        await CompressFolderAsync(subdirectory);
      }
    }
  }
}
