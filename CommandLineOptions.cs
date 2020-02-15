using CommandLine;

namespace DotNetCompression
{
  public class CommandLineOptions
  {
    [Option('s', "source", Required = true, HelpText = "Source directory.")]
    public string Source { get; set; }

    [Option('d', "destination", Required = true, HelpText = "Destination file as DateTime format (see https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).")]
    public string Destination { get; set; }

    [Option('r', "remove-except", Required = false, HelpText = "Only keep the latest archives up to the given count.")]
    public int RemoveExcept { get; set; }
  }
}