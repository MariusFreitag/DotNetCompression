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
    private static void Main(string[] args)
    {
      Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed<CommandLineOptions>(o =>
      {
        var sourceDirectory = new DirectoryInfo(o.Source);
        var destinationFile = new FileInfo(DateTime.Now.ToString(o.Destination));

        var ignoreService = setupIgnoreService(o, sourceDirectory);
        var compressionService = setupCompressionService(ignoreService, sourceDirectory, destinationFile, o.Password);
        var purgationService = setupPurgationService(o, destinationFile);

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
        compressionService.CompressAsync().Wait();
        ClearConsoleLine();
        sw.Stop();

        Console.Write("Compression finished in ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{sw.ElapsedMilliseconds / 1000.0}s");
        Console.ForegroundColor = ConsoleColor.Gray;

        Console.Write("Size: ");
        Console.ForegroundColor = ConsoleColor.Green;
        var destinationFileSize = Math.Round(new FileInfo(destinationFile.FullName).Length / (1024.0 * 1024.0), 2);
        Console.WriteLine($"{destinationFileSize}MB");
        Console.ForegroundColor = ConsoleColor.Gray;

        if (o.RemoveExcept > 0)
        {
          Console.WriteLine("Starting purgation");
          purgationService.Purge();
          Console.WriteLine($"Purgation finished");
        }
      });
    }

    private static IIgnoreService setupIgnoreService(CommandLineOptions options, DirectoryInfo sourceDirectory)
    {
      CombinedIgnoreService combinedIgnoreService = new CombinedIgnoreService();
      if (options.UseGitignoreFiles)
      {
        combinedIgnoreService.addIgnoreService(new GitignoreService(sourceDirectory));
      }
      else if (options.IgnoreFile != null)
      {
        var globIgnoreService = new GlobIgnoreService();
        globIgnoreService.AddFilePatterns(File.ReadAllLines(options.IgnoreFile));
        combinedIgnoreService.addIgnoreService(globIgnoreService);
      }

      return combinedIgnoreService;
    }

    private static ICompressionService setupCompressionService(IIgnoreService ignoreService, DirectoryInfo sourceDirectory, FileInfo destinationFile, string password)
    {
      var zipService = new ZipCompressionService(ignoreService, new CompressionOptions
      {
        Source = sourceDirectory,
        Destination = destinationFile,
        CompressionLevel = CompressionLevel.Zero,
        OverrideDestination = true,
        Password = password
      });

      zipService.Progress += (sender, progressEvent) =>
      {
        ClearConsoleLine();

        var progressString = $"{progressEvent.CurrentCount}/{progressEvent.TotalCount} ";
        var currentElement = progressEvent.CurrentElement.FullName;
        var excessCharacters = currentElement.Length + progressString.Length - Console.WindowWidth;
        if (excessCharacters > 0)
        {
          currentElement = "..." + currentElement.Substring(excessCharacters + 3);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(progressString);
        Console.ForegroundColor = progressEvent.Ignored ? ConsoleColor.Yellow : ConsoleColor.Gray;
        Console.Write(currentElement);
        Console.ForegroundColor = ConsoleColor.Gray;
      };

      return zipService;
    }

    private static IPurgationService setupPurgationService(CommandLineOptions options, FileInfo destinationFile)
    {

      var purgationService = new ZipDateTimePurgationService(new PurgationOptions
      {
        Directory = destinationFile.Directory,
        FileTimeFormat = options.Destination.Split("\\/").Last(),
        KeepCount = options.RemoveExcept,
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

      return purgationService;
    }

    private static void ClearConsoleLine()
    {
      Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
    }
  }
}
