using CommandLine;

namespace Compression
{
  public class CommandLineOptions
  {
    [Option('s', "source", Required = true, HelpText = "Source directory.")]
    public string Source { get; set; }

    [Option('d', "destination", Required = true, HelpText = "Destination file.")]
    public string Destination { get; set; }
  }
}
