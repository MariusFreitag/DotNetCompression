using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using CommandLine;
using DotNetCompression.Compression;
using DotNetCompression.Purgation;

namespace DotNetCompression
{
  class Program
  {
    static void Main(string[] args)
    {
      Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed<CommandLineOptions>(o =>
      {
        var sourceDirectory = new DirectoryInfo(o.Source);
        var destinationFile = new FileInfo(DateTime.Now.ToString(o.Destination));

        CombinedIgnoreService combinedIgnoreService = new CombinedIgnoreService();
        if (o.UseGitignoreFiles)
        {
          combinedIgnoreService.addIgnoreService(new GitignoreService(sourceDirectory));
        }
        else if (o.IgnoreFile != null)
        {
          var globIgnoreService = new GlobIgnoreService();
          globIgnoreService.AddFilePatterns(File.ReadAllLines(o.IgnoreFile));
          combinedIgnoreService.addIgnoreService(globIgnoreService);
        }

        var zipService = new ZipCompressionService(combinedIgnoreService, new CompressionOptions
        {
          Source = sourceDirectory,
          Destination = destinationFile,
          CompressionLevel = CompressionLevel.Zero,
          OverrideDestination = true,
          Password = null
        });

        zipService.Progress += (sender, progressEvent) =>
        {
          Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");

          Console.ForegroundColor = ConsoleColor.Green;
          Console.Write($"{progressEvent.CurrentCount}/{progressEvent.TotalCount} ");
          Console.ForegroundColor = progressEvent.Ignored ? ConsoleColor.Yellow : ConsoleColor.Gray;
          Console.Write(progressEvent.CurrentElement);
          Console.ForegroundColor = ConsoleColor.Gray;
        };

        var purgationService = new ZipDateTimePurgationService(new PurgationOptions
        {
          Directory = destinationFile.Directory,
          FileTimeFormat = o.Destination.Split("\\/").Last(),
          KeepCount = o.RemoveExcept,
          PurgeCorruptFiles = true
        });

        purgationService.Progress += (sender, progressEvent) =>
        {
          switch (progressEvent.Type)
          {
            case PurgationFileType.Corrupt:
              Console.ForegroundColor = ConsoleColor.Red;
              Console.Write("Deleted corrupt file ");
              break;
            case PurgationFileType.Old:
              Console.ForegroundColor = ConsoleColor.Yellow;
              Console.Write("Deleted old file ");
              break;
            case PurgationFileType.Residual:
              Console.ForegroundColor = ConsoleColor.Green;
              Console.Write("Keeping file ");
              break;
          }

          Console.Write(progressEvent.CurrentElement);

          if (progressEvent.Exception != null)
          {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($" with error '{progressEvent.Exception.Message}'");
          }

          Console.WriteLine(".");
          Console.ForegroundColor = ConsoleColor.Gray;
        };

        Console.Write("Starting compression of ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(sourceDirectory.FullName);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" to ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(destinationFile.FullName);
        Console.WriteLine();

        var sw = new Stopwatch();
        sw.Start();
        zipService.CompressAsync().Wait();
        sw.Stop();
        Console.WriteLine();
        Console.Write("Compression finished in ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{sw.ElapsedMilliseconds / 1000.0}s");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("Size: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{Math.Round(new FileInfo(destinationFile.FullName).Length / (1024.0 * 1024.0), 2)}MB");
        Console.ForegroundColor = ConsoleColor.Gray;

        if (o.RemoveExcept > 0)
        {
          Console.WriteLine("Starting purgation");
          sw = new Stopwatch();
          sw.Start();
          purgationService.Purge();
          sw.Stop();
          Console.Write($"Purgation finished in ");
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"{sw.ElapsedMilliseconds / 1000.0}s");
          Console.ForegroundColor = ConsoleColor.Gray;
        }
      });
    }
  }
}
