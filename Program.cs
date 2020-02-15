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

        var zipService = new ZipCompressionService(new GitignoreService(sourceDirectory), new CompressionOptions
        {
          Source = sourceDirectory,
          Destination = destinationFile,
          CompressionLevel = CompressionLevel.Zero,
          OverrideDestination = true,
          Password = null
        });

        zipService.Progress += (sender, progressEvent) =>
        {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.Write($"\r{progressEvent.CurrentCount}/{progressEvent.TotalCount}");
          Console.ForegroundColor = ConsoleColor.Gray;
        };

        var purgationService = new ZipDateTimePurgationService(new PurgationOptions
        {
          Directory = destinationFile.Directory,
          FileTimeFormat = o.Destination.Split("\\/").Last(),
          KeepCount = o.RemoveExcept,
          PurgeCorruptFiles = true
        });

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
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("Compression finished in ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{sw.ElapsedMilliseconds / 1000.0}s");
        Console.ForegroundColor = ConsoleColor.Gray;

        if (o.RemoveExcept > 0)
        {
          Console.WriteLine("Starting purgation");
          sw = new Stopwatch();
          sw.Start();
          purgationService.Purge();
          sw.Stop();
          Console.ForegroundColor = ConsoleColor.Gray;
          Console.Write($"Purgation finished in ");
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"{sw.ElapsedMilliseconds / 1000.0}s");
          Console.ForegroundColor = ConsoleColor.Gray;
        }
      });
    }
  }
}
