using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CommandLine;

namespace Compression
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
          Password = "abc"
        });

        zipService.Progress += (sender, progressEvent) =>
        {
          Console.Write($"\r{progressEvent.CurrentCount}/{progressEvent.TotalCount}");
        };

        Console.WriteLine("Starting");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("...");
        var sw = new Stopwatch();
        sw.Start();
        zipService.CreateAsync().Wait();
        sw.Stop();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"Finished in {sw.ElapsedMilliseconds / 1000.0} s");
      });
    }
  }
}
