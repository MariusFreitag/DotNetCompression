using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using CommandLine;
using DotNetCompression.Compression;
using DotNetCompression.Purgation;
using System.Globalization;

namespace DotNetCompression
{
  internal sealed class Program
  {
    private static void Main(string[] args)
    {
      _ = Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(parsedArguments =>
      {
        DirectoryInfo sourceDirectory = new(parsedArguments.Source);
        FileInfo destinationFile = new(DateTime.Now.ToString(parsedArguments.Destination, CultureInfo.CurrentCulture));

        IIgnoreService ignoreService = SetupIgnoreService(parsedArguments, sourceDirectory);
        ICompressionService compressionService = SetupCompressionService(ignoreService, sourceDirectory, destinationFile, parsedArguments.Password);
        IPurgationService purgationService = SetupPurgationService(parsedArguments, destinationFile);

        Console.Write("Starting compression of ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(sourceDirectory.FullName);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" to ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(destinationFile.FullName);
        Console.WriteLine();

        Stopwatch sw = new();
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
        double destinationFileSize = Math.Round(new FileInfo(destinationFile.FullName).Length / (1024.0 * 1024.0), 2);
        Console.WriteLine($"{destinationFileSize}MB");
        Console.ForegroundColor = ConsoleColor.Gray;

        if (parsedArguments.RemoveExcept > 0)
        {
          Console.WriteLine("Starting purgation");
          purgationService.Purge();
          Console.WriteLine($"Purgation finished");
        }
      });
    }

    private static IIgnoreService SetupIgnoreService(CommandLineOptions options, DirectoryInfo sourceDirectory)
    {
      CombinedIgnoreService combinedIgnoreService = new();
      if (options.UseGitignoreFiles)
      {
        combinedIgnoreService.AddIgnoreService(new GitignoreService(sourceDirectory));
      }
      else if (options.IgnoreFile != null)
      {
        GlobIgnoreService globIgnoreService = new();
        globIgnoreService.AddFilePatterns(File.ReadAllLines(options.IgnoreFile));
        combinedIgnoreService.AddIgnoreService(globIgnoreService);
      }

      return combinedIgnoreService;
    }

    private static ICompressionService SetupCompressionService(IIgnoreService ignoreService, DirectoryInfo sourceDirectory, FileInfo destinationFile, string password)
    {
      ZipCompressionService zipService = new(ignoreService, new CompressionOptions
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

        string progressString = $"{progressEvent.CurrentCount}/{progressEvent.TotalCount} ";
        string currentElement = progressEvent.CurrentElement.FullName;
        int excessCharacters = currentElement.Length + progressString.Length - Console.WindowWidth;
        if (excessCharacters > 0)
        {
          currentElement = "..." + currentElement[(excessCharacters + 3)..];
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(progressString);
        Console.ForegroundColor = progressEvent.Ignored ? ConsoleColor.Yellow : ConsoleColor.Gray;
        Console.Write(currentElement);
        Console.ForegroundColor = ConsoleColor.Gray;
      };

      return zipService;
    }

    private static IPurgationService SetupPurgationService(CommandLineOptions options, FileInfo destinationFile)
    {

      ZipDateTimePurgationService purgationService = new(new PurgationOptions
      {
        Directory = destinationFile.Directory,
        FileTimeFormat = options.Destination.Split("\\/").Last(),
        KeepCount = options.RemoveExcept,
        PurgeCorruptFiles = true,
        Password = options.Password
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
          default:
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
